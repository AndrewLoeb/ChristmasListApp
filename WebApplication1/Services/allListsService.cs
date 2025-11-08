using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class allListsService
    {

        public List<UserModel> userList = new List<UserModel>();
        public List<ListModel> AllLists = new List<ListModel>();
        public DateTime LastRefreshed { get; private set; }

        public allListsService()
        {
            LastRefreshed = DateTime.Now;
        }

        public Task SetAllLists (List<UserModel> userListToSet, List<ListModel> AllListsToSet)
        {
            userList = userListToSet;
            AllLists = AllListsToSet;
            LastRefreshed = DateTime.Now;
            return Task.CompletedTask;
        }

        public Task<bool> RefreshAllData(googleSheetsListService listService)
        {
            try
            {
                // Reload all lists from Google Sheets
                var refreshedLists = listService.GetAllLists(userList);

                // Update the AllLists property
                AllLists = refreshedLists;

                // Update timestamp
                LastRefreshed = DateTime.Now;

                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing data: {ex.Message}");
                return Task.FromResult(false);
            }
        }


    }
}

