﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1.Services
{
    public class userIdService
    {
        public string userId = "";

        public userIdService()
        {

        }

        public async Task SetUser(string userIdToSet)
        {
            userId = userIdToSet;

        }


    }
}

