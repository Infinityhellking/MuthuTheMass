﻿using AutoMapper;
using CarParkingBookingDatabase.BookingDBContext;
using CarParkingBookingDatabase.DBModel;
using CarParkingBookingVM.Authorization;
using CarParkingBookingVM.Login;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ValidateCarParkingDetails.ValidateAuthorization
{
    public interface IAuthorization 
    {
        Task<bool?> InsertLoginDetials(SignUpVM? SignUpDetials);

        Task<AuthorizedLoginVM> VerifyUser(LoginVM login);
        Task<bool?> InsertDealerDetails(DealerSignUpVM dealerSign);
        Task<(AuthorizedDealerLoginVM, bool?)> VerifyDealer(DealerLogin dealer);


    }

    public class Authorization : IAuthorization
    {
        private readonly CarParkingBookingDBContext dBContext;
        private readonly IMapper mapper;

        public Authorization(CarParkingBookingDBContext carParkingBookingDB,IMapper _mapper)
        {
            dBContext = carParkingBookingDB;
            mapper = _mapper;
        }

        // User Authorization

        public async Task<bool?> InsertLoginDetials(SignUpVM? SignUpDetials)
        {
            if(SignUpDetials is not null) 
            {
                if (!string.IsNullOrEmpty(SignUpDetials.Password!)
                    || !(SignUpDetials.MobileNumber!.Length == 10)
                    || !SignUpDetials.Email!.Contains("@")
                    || !SignUpDetials.Email.EndsWith(".com"))
                {
                    return await Task.FromResult(false);
                }
                else
                {
                    var duplicate = dBContext.userDetails.FirstOrDefault(v => v.Email == SignUpDetials.Email);
                    if (duplicate is null) 
                    {
                        var data =mapper.Map<UserDetails>(SignUpDetials);

                        await dBContext.userDetails.AddAsync(data);
                        await dBContext.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        return null;
                    }

                }

            }
            return await Task.FromResult(false);
        }

        public Task<AuthorizedLoginVM> VerifyUser(LoginVM login)
        {
            if(login.Email is not null && login.Password is not null)
            {
                var data = dBContext.userDetails.FirstOrDefault(y =>y.Email == login.Email);
                if (data is not null && data.Password.Equals(login.Password))
                {
                    var result = new AuthorizedLoginVM()
                    {
                        UserName = data.Name!,
                        Email = data.Email,
                        Access = data.Rights
                    };

                    return Task.FromResult(result);
                }
            }
            return Task.FromResult<AuthorizedLoginVM>(null!);
        }

        //dealer Authorization

        public async Task<bool?> InsertDealerDetails(DealerSignUpVM dealerSign)
        {
            if (dealerSign is not null)
            {
                if (!string.IsNullOrEmpty(dealerSign.Password!)
                    || !(dealerSign.PhoneNo!.Length == 10)
                    || !dealerSign.Email!.Contains("@")
                    || !dealerSign.Email.EndsWith(".com"))
                {
                    return await Task.FromResult(false);
                }
                else
                {
                    var duplicate = dBContext.dealerDetails.FirstOrDefault(v => v.DealerEmail == dealerSign.Email);
                    if (duplicate is null)
                    {
                        var data = mapper.Map<UserDetails>(dealerSign);

                        await dBContext.userDetails.AddAsync(data);
                        await dBContext.SaveChangesAsync();
                    }
                    else
                    {
                        return null;
                    }

                    return await Task.FromResult(true);
                }

            }

            return true;
        }

        public async Task<(AuthorizedDealerLoginVM?,bool?)> VerifyDealer(DealerLogin dealer)
        {
            var data = dBContext.dealerDetails.FirstOrDefault(h=>h.DealerEmail == dealer.Email);
            if (data is null)
            {
                return (null,null);
            }
            else if (data.DealerPassword != dealer.Password) 
            {
                return (null,false);
            }
            else
            {
                return (mapper.Map<AuthorizedDealerLoginVM>(data),null);
                
            }
        }
    }
}
