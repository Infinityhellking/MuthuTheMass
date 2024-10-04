﻿using Auth.DataAccess.AuthDbContext;
using AutoMapper;
using CarParkingBookingDatabase.BookingDBContext;
using CarParkingBookingDatabase.DBModel;
using CarParkingBookingVM.Authorization;
using CarParkingBookingVM.Login;
using Microsoft.AspNetCore.Identity;

namespace ValidateCarParkingDetails.ValidateAuthorization
{
    public interface IAuthorization 
    {
        Task<bool> UpsertLoginDetials(SignUpVM? SignUpDetials);

        Task<AuthorizedLoginVM> VerifyUser(LoginVM login);

    }

    public class Authorization : IAuthorization
    {
        private readonly AuthDbContext dBContext;
        private readonly IMapper mapper;

        public Authorization(AuthDbContext carParkingBookingDB,IMapper _mapper)
        {
            dBContext = carParkingBookingDB;
            mapper = _mapper;
        }

        public async Task<bool> UpsertLoginDetials(SignUpVM? SignUpDetials)
        {
            if(SignUpDetials is not null) 
            {
                if (!(SignUpDetials.Password!.Equals(SignUpDetials.ConfirmPassword))
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
                    }
                    else
                    {
                        mapper.Map(SignUpDetials, duplicate);
                        dBContext.userDetails.Update(duplicate);
                        await dBContext.SaveChangesAsync();
                    }

                    return await Task.FromResult(true);
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

       
    }
}
