﻿using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Toss.Server.Data;
using Toss.Server.Models;
using Toss.Server.Models.Tosses;
using Toss.Server.Services;
using Toss.Shared.Tosses;

namespace Toss.Server.Controllers
{
    public class TossCreateCommandHandler : IRequestHandler<TossCreateCommand>
    {
        private readonly ICosmosDBTemplate<TossEntity> _dbTemplate;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> userManager;
        private IStripeClient stripeClient;
        private IMediator mediator;

        public TossCreateCommandHandler(ICosmosDBTemplate<TossEntity> cosmosTemplate, IHttpContextAccessor httpContextAccessor, IStripeClient stripeClient, UserManager<ApplicationUser> userManager,
            IMediator mediator)
        {
            this.mediator = mediator;
            _dbTemplate = cosmosTemplate;
            _httpContextAccessor = httpContextAccessor;
            this.stripeClient = stripeClient;
            this.userManager = userManager;
        }

        public async Task<Unit> Handle(TossCreateCommand command, CancellationToken cancellationToken)
        {
            TossEntity toss;
            var user = _httpContextAccessor.HttpContext.User;
            if (!command.SponsoredDisplayedCount.HasValue)
                toss = new TossEntity(
                    command.Content,
                    user.UserId(),
                    DateTimeOffset.Now);
            else
                toss = new SponsoredTossEntity(
                    command.Content,
                    user.UserId(),
                    DateTimeOffset.Now,
                    command.SponsoredDisplayedCount.Value);
            toss.UserName = user.Identity.Name;
            var matchCollection = TossCreateCommand.TagRegex.Matches(toss.Content);
            toss.Tags = matchCollection.Select(m => m.Groups[1].Value).ToList();
            toss.UserIp = _httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
            toss.Id = await _dbTemplate.Insert(toss);
            if (command.SponsoredDisplayedCount.HasValue)
            {
                ApplicationUser applicationUser = (await userManager.GetUserAsync(user));
                var paymentResult = await stripeClient.Charge(command.StripeChargeToken, command.SponsoredDisplayedCount.Value * TossCreateCommand.CtsCostPerDisplay, "Payment for sponsored Toss #" + toss.Id, applicationUser.Email);
                if (!paymentResult)
                {
                    await _dbTemplate.Delete(toss.Id);
                    throw new InvalidOperationException("Payment error on sponsored Toss ");
                }

            }
            await mediator.Publish(new TossCreated(toss));
            return Unit.Value;
        }
    }
}
