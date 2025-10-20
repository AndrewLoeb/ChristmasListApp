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

        public allListsService()
        {

        }

        public async Task SetAllLists (List<UserModel> userListToSet, List<ListModel> AllListsToSet)
        {
            userList = userListToSet;
            AllLists = AllListsToSet;

        }


    }
}

