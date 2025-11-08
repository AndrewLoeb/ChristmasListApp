using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
// Added for this app from https://dev.to/zoltanhalasz/google-sheets-with-net-core-razor-pages-crud-tutorial-38ee
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
// Added for me
using WebApplication1.Models;

namespace WebApplication1.Services
{
    public class googleSheetsListService
    {

        private readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
        private readonly string ApplicationName = "Christmas List Data";
        private readonly string usersSheet = "Users";
        private readonly string itemSheet = "Items";
        private readonly string SpreadsheetId;
        private SheetsService service;

        public googleSheetsListService(IConfiguration configuration)
        {
            SpreadsheetId = configuration["GoogleSheets:ListSpreadsheetId"];
            Init();
        }

        private void Init()
        {
            GoogleCredential credential;
            //Reading Credentials File...
            using (var stream = new FileStream("app_client_secret.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped(Scopes);
            }
            // Creating Google Sheets API service...
            service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        private T ExecuteWithRetry<T>(Func<T> operation, string operationName)
        {
            int maxRetries = 3;
            int retryCount = 0;
            Exception lastException = null;

            while (retryCount < maxRetries)
            {
                try
                {
                    Console.WriteLine($"Attempting {operationName} (attempt {retryCount + 1}/{maxRetries})");
                    return operation();
                }
                catch (Exception ex)
                {
                    retryCount++;
                    lastException = ex;
                    Console.WriteLine($"Error on attempt {retryCount}/{maxRetries} for {operationName}: {ex.Message}");

                    if (retryCount < maxRetries)
                    {
                        // Wait before retrying (exponential backoff: 1s, 2s, 4s)
                        int delayMs = (int)Math.Pow(2, retryCount - 1) * 1000;
                        Console.WriteLine($"Waiting {delayMs}ms before retry...");
                        System.Threading.Thread.Sleep(delayMs);
                    }
                }
            }

            // All retries failed
            Console.WriteLine($"All {maxRetries} attempts failed for {operationName}");
            throw new Exception($"Failed to {operationName} after {maxRetries} attempts. Please contact Andrew for assistance.", lastException);
        }

        public List<UserModel> Users_GetList()
        {
            return ExecuteWithRetry(() =>
            {
                var myUserList = new List<UserModel>();
                var range = $"{usersSheet}!A:E";
                int j = 0;
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(SpreadsheetId, range);
                // Ecexuting Read Operation...
                var response = request.Execute();
                // Getting all records from Column A to E...
                IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    j++;
                    if (j > 1)
                    {
                        var myInv = new UserModel()
                        {
                            Id = Int32.Parse(row[0].ToString()),
                            Family = row[1].ToString(),
                            Name = row[2].ToString(),
                            Password = row[3].ToString(),
                            Notes = row.Count > 4 && row[4] != null ? row[4].ToString() : "",
                        };

                        myUserList.Add(myInv);
                    }
                }
            }
                else
                {
                    Console.WriteLine("Error reading data.");
                }

                return myUserList;
            }, "load users");
        }
        public void UpdateNotes(string userId, string newNotes)
        {
            //var myInvList = Spices_GetList();
            List<UserModel> AllUsers = Users_GetList();
            int userId2 = AllUsers.FirstOrDefault(u => u.Name == userId).Id;

            var range = $"{usersSheet}!E{userId2 + 1}:E{userId2 + 1}";
            var valueRange = new ValueRange();
            var oblist = new List<object>() { newNotes };
            valueRange.Values = new List<IList<object>> { oblist };
            // Performing Update Operation...
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();
        }
        public List<ItemModel> GetAllItems()
        {
            return ExecuteWithRetry(() =>
            {
                var AllItems = new List<ItemModel>();
                var range = $"{itemSheet}!A:L";  // Extended to column L for new metadata fields
                int j = 0;
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(SpreadsheetId, range);
                // Ecexuting Read Operation...
                var response = request.Execute();
                // Getting all records from Column A to L...
                IList<IList<object>> values = response.Values;
                if (values != null && values.Count > 0)
                {
                    foreach (var row in values)
                    {
                        j++;
                        if (j > 1)
                        {
                            var myItem = new ItemModel()
                            {
                                ItemId = Int32.Parse(row[0].ToString()),
                                Name = row[1].ToString(),
                                Item = row[2].ToString(),
                                Notes = row[3].ToString(),
                                Link = row[4].ToString(),
                                DateUpdated = row[5].ToString(),
                                Claimer = row[6].ToString(),
                                DateClaimed = row[7].ToString(),
                                Active = Int32.Parse(row[8].ToString()),
                                // Read new metadata fields (columns J, K, L) - handle missing data gracefully
                                ImageUrl = row.Count > 9 && row[9] != null ? row[9].ToString() : "",
                                Price = row.Count > 10 && row[10] != null && !string.IsNullOrWhiteSpace(row[10].ToString())
                                    ? decimal.Parse(row[10].ToString())
                                    : (decimal?)null,
                                MetadataFetchedDate = row.Count > 11 && row[11] != null ? row[11].ToString() : "",
                            };

                            AllItems.Add(myItem);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Error reading data.");
                }

                return AllItems;
            }, "load all items");
        }
        public List<ItemModel> GetMyList(string userId)
        {
            return ExecuteWithRetry(() =>
            {
                System.Diagnostics.Debug.WriteLine($"List request for: {userId}");
                var MyList = new List<ItemModel>();
                var range = $"{itemSheet}!A:L";  // Extended to column L for new metadata fields
                int j = 0;
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(SpreadsheetId, range);
                // Ecexuting Read Operation...
                var response = request.Execute();
                // Getting all records from Column A to L...
                IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var row in values)
                {
                    j++;
                    if ((j > 1) & (row[1].ToString().Equals(userId)))
                    {
                        var myItem = new ItemModel()
                        {
                            ItemId = Int32.Parse(row[0].ToString()),
                            Name = row[1].ToString(),
                            Item = row[2].ToString(),
                            Notes = row[3].ToString(),
                            Link = row[4].ToString(),
                            DateUpdated = row[5].ToString(),
                            Claimer = row[6].ToString(),
                            DateClaimed = row[7].ToString(),
                            Active = Int32.Parse(row[8].ToString()),
                            // Read new metadata fields (columns J, K, L) - handle missing data gracefully
                            ImageUrl = row.Count > 9 && row[9] != null ? row[9].ToString() : "",
                            Price = row.Count > 10 && row[10] != null && !string.IsNullOrWhiteSpace(row[10].ToString())
                                ? decimal.Parse(row[10].ToString())
                                : (decimal?)null,
                            MetadataFetchedDate = row.Count > 11 && row[11] != null ? row[11].ToString() : "",
                        };

                        if (myItem.Active == 1)
                        {
                            MyList.Add(myItem);
                        }
                    }
                }
            }
                else
                {
                    Console.WriteLine("Error reading data.");
                }

                return MyList;
            }, $"load items for {userId}");
        }
        //WIP
        public List<ListModel> GetAllLists(List<UserModel> userList)
        {
            var AllLists = new List<ListModel>();

            // Load all items once instead of calling GetMyList for each user
            var allItems = GetAllItems();

            foreach(UserModel user in userList)
            {
                int itemsListed = 0;
                int itemsClaimed = 0;
                var lastUpdated = DateTime.Parse("2022-12-26T00:00:00.000+00:00");

                // Filter items for this user from the already-loaded list
                List<ItemModel> MyList = allItems
                    .Where(i => i.Name == user.Name && i.Active == 1)
                    .ToList();

                foreach (ItemModel item in MyList)
                {
                    itemsListed++;
                    if (item.Claimer != "") { itemsClaimed++; }
                    if (DateTime.Parse(item.DateUpdated) > lastUpdated) { lastUpdated = DateTime.Parse(item.DateUpdated); }
                }
                string dropDownStr = String.Format("{0}-Items: {1,-2}; Unclaimed Items: {2,-2}; Last Updated: {3}", user.Name.PadRight(15, '-'), itemsListed, itemsListed - itemsClaimed, lastUpdated.ToString("d"));
                ListModel myList = new ListModel()
                {
                    List = MyList,
                    Name = user.Name,
                    itemsListed = itemsListed,
                    itemsClaimed = itemsClaimed,
                    lastUpdated = lastUpdated,
                    dropDownStr = dropDownStr
                };
                AllLists.Add(myList);
            }
            return AllLists;
        }

        public void AddItem(string userId, string newItemItem, string newItemNotes, string newItemLink, string imageUrl = "", decimal? price = null, string metadataFetchedDate = "")
        {
            // Specifying Column Range for reading...
            var range = $"{itemSheet}!A:L";  // Extended to column L for new metadata fields
            var valueRange = new ValueRange();

            //var myInvList = Spices_GetList();
            List<ItemModel> AllLists = GetAllItems();
            int maxId = AllLists.Max(i => i.ItemId);

            // Columns: A-I (existing) + J-L (new metadata fields)
            var newItem = new List<object>() {
                maxId+1,              // A: ItemId
                userId,               // B: Name
                newItemItem,          // C: Item
                newItemNotes,         // D: Notes
                newItemLink,          // E: Link
                DateTime.Now,         // F: DateUpdated
                "",                   // G: Claimer
                "",                   // H: DateClaimed
                1,                    // I: Active
                imageUrl,             // J: ImageUrl
                price?.ToString() ?? "", // K: Price (convert to string for Google Sheets)
                metadataFetchedDate   // L: MetadataFetchedDate
            };
            valueRange.Values = new List<IList<object>> { newItem };
            // Append the above record...
            var appendRequest = service.Spreadsheets.Values.Append(valueRange, SpreadsheetId, range);
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = appendRequest.Execute();
        }

        public void UpdateItem(int itemId, string itemItem, string itemNotes, string itemLink, string imageUrl = null, decimal? price = null, string metadataFetchedDate = null)
        {
            // Update columns C-F (Item, Notes, Link, DateUpdated)
            var range1 = $"{itemSheet}!C{itemId + 1}:F{itemId + 1}";
            var valueRange1 = new ValueRange();
            var oblist1 = new List<object>() { itemItem, itemNotes, itemLink, DateTime.Now };
            valueRange1.Values = new List<IList<object>> { oblist1 };

            var updateRequest1 = service.Spreadsheets.Values.Update(valueRange1, SpreadsheetId, range1);
            updateRequest1.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            updateRequest1.Execute();

            // Update columns J-L (ImageUrl, Price, MetadataFetchedDate) if provided
            if (imageUrl != null || price != null || metadataFetchedDate != null)
            {
                var range2 = $"{itemSheet}!J{itemId + 1}:L{itemId + 1}";
                var valueRange2 = new ValueRange();
                var oblist2 = new List<object>() {
                    imageUrl ?? "",              // J: ImageUrl
                    price?.ToString() ?? "",     // K: Price
                    metadataFetchedDate ?? ""    // L: MetadataFetchedDate
                };
                valueRange2.Values = new List<IList<object>> { oblist2 };

                var updateRequest2 = service.Spreadsheets.Values.Update(valueRange2, SpreadsheetId, range2);
                updateRequest2.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
                updateRequest2.Execute();
            }
        }

        public void DeleteItem(int itemId)
        {
            var range = $"{itemSheet}!I{itemId+1}:I{itemId+1}";
            var valueRange = new ValueRange();
            var oblist = new List<object>() { 0 };
            valueRange.Values = new List<IList<object>> { oblist };
            // Performing Update Operation...
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();
        }

        public void ClaimItem(int itemId, string claimerId)
        {
            var range = $"{itemSheet}!G{itemId + 1}:H{itemId + 1}";
            var valueRange = new ValueRange();
            var oblist = new List<object>() { claimerId, DateTime.Now };
            valueRange.Values = new List<IList<object>> { oblist };
            // Performing Update Operation...
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();
        }
        public void UnclaimItem(int itemId)
        {
            var range = $"{itemSheet}!G{itemId + 1}:H{itemId + 1}";
            var valueRange = new ValueRange();
            var oblist = new List<object>() { "", "" };
            valueRange.Values = new List<IList<object>> { oblist };
            // Performing Update Operation...
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();
        }

        public ItemModel GetItemById(int itemId)
        {
            return ExecuteWithRetry(() =>
            {
                // Read the specific row for this itemId
                var range = $"{itemSheet}!A{itemId + 1}:L{itemId + 1}";
                SpreadsheetsResource.ValuesResource.GetRequest request =
                        service.Spreadsheets.Values.Get(SpreadsheetId, range);
                var response = request.Execute();
                IList<IList<object>> values = response.Values;

                if (values != null && values.Count > 0)
                {
                    var row = values[0];
                    var item = new ItemModel()
                    {
                        ItemId = Int32.Parse(row[0].ToString()),
                        Name = row[1].ToString(),
                        Item = row[2].ToString(),
                        Notes = row[3].ToString(),
                        Link = row[4].ToString(),
                        DateUpdated = row[5].ToString(),
                        Claimer = row[6].ToString(),
                        DateClaimed = row[7].ToString(),
                        Active = Int32.Parse(row[8].ToString()),
                        ImageUrl = row.Count > 9 && row[9] != null ? row[9].ToString() : "",
                        Price = row.Count > 10 && row[10] != null && !string.IsNullOrWhiteSpace(row[10].ToString())
                            ? decimal.Parse(row[10].ToString())
                            : (decimal?)null,
                        MetadataFetchedDate = row.Count > 11 && row[11] != null ? row[11].ToString() : "",
                    };
                    return item;
                }
                else
                {
                    throw new Exception($"Item with ID {itemId} not found");
                }
            }, $"get item {itemId}");
        }

        public void UpdateInvoice(SpiceModel Spice)
        {
            int rowID = 0;
            var range = $"{itemSheet}!A:D";
            int j = 0;
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(SpreadsheetId, range);
            var response = request.Execute();
            IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var invoicerow in values)
                {
                    j++;
                    if (j > 1)
                    {
                        var Id = Int32.Parse(invoicerow[0].ToString());
                        if (Id == Spice.Id) rowID = j;
                    }

                }
            }

            var range2 = $"{itemSheet}!A{rowID}:D{rowID}";
            var valueRange = new ValueRange();
            var oblist = new List<object>() { Spice.Id, Spice.Name, Spice.Category1, Spice.Category2 };
            valueRange.Values = new List<IList<object>> { oblist };
            // Performing Update Operation...
            var updateRequest = service.Spreadsheets.Values.Update(valueRange, SpreadsheetId, range2);
            updateRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
            var appendReponse = updateRequest.Execute();
        }

        public void ClearInvoiceID(int Id)
        {
            int rowID = Id + 1;
            var range = $"{itemSheet}!A{rowID}:D{rowID}";
            var requestBody = new ClearValuesRequest();
            var deleteRequest = service.Spreadsheets.Values.Clear(requestBody, SpreadsheetId, range);
            var deleteReponse = deleteRequest.Execute();
        }


        public void DeleteInvoice(SpiceModel inv)
        {

            int row = 0;
            var range = $"{itemSheet}!A:D";
            int j = 0;
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(SpreadsheetId, range);
            var response = request.Execute();
            IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (var invoicerow in values)
                {
                    j++;
                    if (j > 1)
                    {
                        var Id = Int32.Parse(invoicerow[0].ToString());
                        if (Id == inv.Id) row = j - 1;
                    }

                }
            }


            Request RequestBody = new Request()
            {
                DeleteDimension = new DeleteDimensionRequest()
                {
                    Range = new DimensionRange()
                    {
                        SheetId = 461012880,
                        Dimension = "ROWS",
                        StartIndex = row,
                        EndIndex = row + 1
                    }
                }
            };

            List<Request> RequestContainer = new List<Request>();
            RequestContainer.Add(RequestBody);

            BatchUpdateSpreadsheetRequest DeleteRequest = new BatchUpdateSpreadsheetRequest();
            DeleteRequest.Requests = RequestContainer;

            SpreadsheetsResource.BatchUpdateRequest Deletion = new SpreadsheetsResource.BatchUpdateRequest(service, DeleteRequest, SpreadsheetId);
            Deletion.Execute();

        }

    }
}