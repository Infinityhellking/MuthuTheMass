﻿using AutoMapper;
using CarParkingBookingDatabase.BookingDBContext;
using CarParkingBookingDatabase.DBModel;
using CarParkingBookingDatabase.SqlHelper;
using CarParkingBookingVM.VM_S.Dealers;
using Microsoft.EntityFrameworkCore;

namespace ValidateCarParkingDetails.ValidateAuthorization
{

    public interface IDealerData
    {
        Task<List<DealerVM>> SearchData(Filter filter);

        Task<bool> AddDealerData(DealerVM dealerVM);
    }

    public class DealerData : IDealerData
    {
        private readonly CarParkingBookingDBContext dbContext;
        private readonly IMapper mapper;

        public DealerData(CarParkingBookingDBContext _dbContext,IMapper _mapper)
        {
            mapper = _mapper;
            dbContext = _dbContext;
        }

        public async Task<bool> AddDealerData(DealerVM dealerVM)
        {
            if (!string.IsNullOrEmpty(dealerVM.DealerName))
            {
                var data = mapper.Map<DealerDetails>(dealerVM);
                dbContext.dealerDetails.Add(data);
                dbContext.SaveChanges();
                return true;

            }
            else
            {
                return false;
            }
        }

        public Task<List<DealerVM>> SearchData(Filter filter)
        {
            List<DealerDetails>? data;
            var queryString = "SELECT * FROM dealerDetails ";


            if (filter.filters.Any())
            {
                if (filter.filters.Any(b=>b.key.Contains("timing")))
                {
                    queryString += " CROSS APPLY STRING_SPLIT(DealerTiming, '-') AS TimingSplit ";
                }

                foreach (var search in filter.filters)
                {
                    if (search.key.ToLower().Contains("address"))
                    {
                        queryString = SqlHelper.clause(queryString, $" LOWER(DealerAddress) LIKE '%{search.value.ToLower()}%'");
                    }
                    if (search.key.ToLower().Contains("timing"))
                    {
                        if (search.key.ToLower().Contains("timingstart"))
                        {
                            queryString = SqlHelper.clause(queryString, $" TRIM(TimingSplit.value) = '{search.value}';");
                        }
                        if (search.key.ToLower().Contains("timingstop"))
                        {
                            queryString = SqlHelper.clause(queryString, $" TRIM(TimingSplit.value) = '{search.value}';)");
                        }
                    }
                }

                
            }

            var query = dbContext.dealerDetails.FromSqlRaw(queryString);
            data = query.ToList();

            
            var result = mapper.Map<List<DealerVM>>(data);

            return Task.FromResult(result);
        }


        private string TimingSeperation(string date,int count)
        {
            var t = date.Substring(0, date.IndexOf("-"));
            switch (count)
            {
                case 1:
                    return date.Split("-").First();
                case 2:
                    return date.Split("-").Last();

            }
            return string.Empty;
        }
    }
}
