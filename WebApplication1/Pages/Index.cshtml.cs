using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Pages
{
    public class IndexModel : PageModel
    {

        public IndexModel(googleSheetsListService listService, googleSheetsSpiceService spiceService)
        {
            ListService = listService;
            SpiceService = spiceService;
        }


        public googleSheetsListService ListService { get; }
        public IEnumerable<UserModel> Users { get; private set; }
        public googleSheetsSpiceService SpiceService { get; }
        public bool showLists { get; set; }
        public void OnGet()
        {
            Users = ListService.Users_GetList();
            showLists = false;
        }


        void Set_ShowLists()
        {
            showLists = true;
        }

    }
}
