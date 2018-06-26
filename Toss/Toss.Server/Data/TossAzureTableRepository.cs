﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Toss.Shared;

namespace Toss.Server.Data
{
    /// <summary>
    /// Store and retreive Toss from an Azure Table Storage
    /// </summary>
    public class TossCosmosRepository : ITossRepository
    {

        private const string collectionId = "Toss";
        public TossCosmosRepository(DocumentClient documentClient, string databaseId)
        {
            _documentClient = documentClient;
            _database = new Lazy<Task<ResourceResponse<Database>>>(() => 
                _documentClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = databaseId })
            );
            _collection = new Lazy<Task<ResourceResponse<DocumentCollection>>>(async () => 
                await _documentClient.CreateDocumentCollectionIfNotExistsAsync(
                    (await  GetDatabase()).SelfLink,
                    new DocumentCollection(){Id = collectionId }
                )
            );
        }
        private readonly DocumentClient _documentClient;
        private readonly Lazy<Task<ResourceResponse<Database>>> _database;
        private readonly Lazy<Task<ResourceResponse<DocumentCollection>>> _collection;

        private async Task<Database> GetDatabase()
        {
            return (await _database.Value).Resource;
        }
        private async Task<DocumentCollection> GetCollection()
        {
            return (await _collection.Value).Resource;
        }

         private async Task<Uri> GetCollectionUri()
        {
            return new Uri((await GetCollection()).SelfLink);
        }
        /// <summary>
        /// Return the X last toss created.
        /// If none was created an empty list is returned
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<IEnumerable<TossLastQueryItem>> Last(int count, string hashTag)
        {
            return await GetAllResultsAsync(_documentClient.CreateDocumentQuery<OneTossEntity>(await GetCollectionUri())
                .Where(t => t.Content.Contains("#" + hashTag))
                .OrderByDescending(t => t.CreatedOn)
                .Take(count)
                .Select(t => new TossLastQueryItem()
                {
                    Content = t.Content,
                    CreatedOn = t.CreatedOn,
                    Id = t.Id,
                    UserName = t.UserName
                })
                .AsDocumentQuery());


        }



        /// <summary>
        /// Saves a toss on the DB, the command should be validated before, 
        /// </summary>
        /// <param name="createCommand"></param>
        /// <returns></returns>
        public async Task Create(TossCreateCommand createCommand)
        {

            var toss = new OneTossEntity(createCommand.Content, createCommand.UserId, createCommand.CreatedOn);
            var tasks = new List<Task>();
            tasks.Add(_documentClient.CreateDocumentAsync(await GetCollectionUri(), toss));
            await Task.WhenAll(tasks);
        }

        private async static Task<T[]> GetAllResultsAsync<T>(IDocumentQuery<T> queryAll)
        {
            var list = new List<T>();

            while (queryAll.HasMoreResults)
            {
                var docs = await queryAll.ExecuteNextAsync<T>();

                foreach (var d in docs)
                {
                    list.Add(d);
                }
            }

            return list.ToArray();
        }
    }
}
