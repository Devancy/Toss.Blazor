﻿using MediatR;
using Microsoft.AspNetCore.Identity;
using Raven.Client.Documents;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Toss.Server.Data;
using Toss.Server.Models;
using Toss.Server.Services;
using Xunit;

namespace Toss.Tests.Infrastructure
{
    public class BaseTest : RavenTestDriver, IAsyncLifetime
    {

        protected IMediator _mediator;
        protected UserManager<ApplicationUser> _userManager;
        protected FakeEmailSender _emailSender;
        protected ServiceProviderInitializer serviceProviderInitializer;
        protected IDocumentStore documentStore;

        protected RavenDBIdUtil _ravenDbIdUtil;

        public ClaimsPrincipal ClaimPrincipal { get; private set; }

        public BaseTest()
        {
            serviceProviderInitializer = new ServiceProviderInitializer();
            documentStore = this.GetDocumentStore(new GetDocumentStoreOptions()
            {
                WaitForIndexingTimeout = TimeSpan.FromSeconds(60)
            },
            database: "Toss");
            serviceProviderInitializer.BuildServiceProvider(documentStore);
            _mediator = serviceProviderInitializer.GetInstance<IMediator>();
            _ravenDbIdUtil = serviceProviderInitializer.GetInstance<RavenDBIdUtil>();
            _userManager = serviceProviderInitializer.GetInstance<UserManager<ApplicationUser>>();
            _emailSender = serviceProviderInitializer.GetInstance<IEmailSender>() as FakeEmailSender;
        }



        protected async Task EditCurrentUser(Action<ApplicationUser> toDo)
        {
            var user = await _userManager.GetUserAsync(serviceProviderInitializer.ClaimPrincipal);
            toDo(user);
            await _userManager.UpdateAsync(user);
        }

        protected async Task SaveAndWait()
        {

            await GetSession().SaveChangesAsync();
            base.WaitForIndexing(documentStore);
        }

        protected IAsyncDocumentSession GetSession()
        {
            return serviceProviderInitializer.GetInstance<IAsyncDocumentSession>();

        }

        public async Task InitializeAsync()
        {
            await CreateTestUser();
            await SaveAndWait();

        }
        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }



        public async Task CreateTestUser()
        {
            await CreateNewUserIfNotExists("username");
        }

        public async Task CreateNewUserIfNotExists(string userName)
        {
            var existing = await _userManager.FindByNameAsync(userName);
            if (existing == null)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    UserName = userName,
                    Email = userName + "@yopmail.com",
                    EmailConfirmed = true
                };
                await _userManager.CreateAsync(user);

                existing = user;
            }
            serviceProviderInitializer.ChangeCurrentUser(existing);
        }


    }
}
