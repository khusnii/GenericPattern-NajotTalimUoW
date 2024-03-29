﻿using AutoMapper;
using NajotTalimUoW.Data.IRepositories;
using NajotTalimUoW.Domain.Common;
using NajotTalimUoW.Domain.Commons;
using NajotTalimUoW.Domain.Configurations;
using NajotTalimUoW.Domain.Entities.Students;
using NajotTalimUoW.Domain.Enums;
using NajotTalimUoW.Service.DTOs.Students;
using NajotTalimUoW.Service.Extensions;
using NajotTalimUoW.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NajotTalimUoW.Service.Services
{
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;

        public StudentService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
        }

        public async Task<BaseResponse<Student>> CreateAsync(StudentForCreationDto studentDto)
        {
            var response = new BaseResponse<Student>();

            // check for student
            var existStudent = await unitOfWork.Students.GetAsync(p => p.Phone == studentDto.Phone);
            if (existStudent is not null)
            {
                response.Error = new ErrorResponse(400, "User is exist");
                return response;
            }

            var existGroup = unitOfWork.Groups.GetAsync(p => p.Id == studentDto.GroupId);
            if (existGroup is null)
            {
                response.Error = new ErrorResponse(404, "Group not found");
                return response;
            }


            //create after checking success
            var mappedStudent = mapper.Map<Student>(studentDto);

            var result = await unitOfWork.Students.CreateAsync(mappedStudent);

            await unitOfWork.SaveChangesAsync();

            response.Data = result;

            return response;
        }

        public async Task<BaseResponse<bool>> DeleteAsync(Expression<Func<Student, bool>> expression)
        {
            var response = new BaseResponse<bool>();

            //Check for exist student
            var existStudent = await unitOfWork.Students.GetAsync(expression);
            if (existStudent is null)
            {
                response.Error = new ErrorResponse(404, "User not found");
            }

            var result = await unitOfWork.Students.UpdateAsync(existStudent);

            await unitOfWork.SaveChangesAsync();

            response.Data = true;

            return response;
        }

        public async Task<BaseResponse<IEnumerable<Student>>> GetAllAsync(PaginationParams @params, Expression<Func<Student, bool>> expression = null)
        {
            var response = new BaseResponse<IEnumerable<Student>>();

            var students = await unitOfWork.Students.GetAllAsync(expression);


            response.Data = students.ToPagedList(@params);

            return response;
        }

        public async Task<BaseResponse<Student>> GetAsync(Expression<Func<Student, bool>> expression)
        {
            var response = new BaseResponse<Student>();

            var student = await unitOfWork.Students.GetAsync(expression);
            if (student is null)
            {
                response.Error = new ErrorResponse(404, "User not found");
                return response;
            }

            response.Data = student;

            return response;
        }

        public async Task<BaseResponse<Student>> UpdateAsync(Guid id, StudentForCreationDto studentDto)
        {
            var response = new BaseResponse<Student>();

            //check for exist student

            var student = await unitOfWork.Students.GetAsync(p => p.Id == id && p.State != ItemState.Deleted);

            if (student is null)
            {
                response.Error = new ErrorResponse(404, "User not found");
                return response;
            }


            // check for exist group
            var group = await unitOfWork.Groups.GetAsync(p => p.Id == studentDto.GroupId);
            if (group is null)
            {
                response.Error = new ErrorResponse(404, "User not found");
                return response;
            }



            student.FirstName = studentDto.FirstName;
            student.Phone = studentDto.Phone;
            student.LastName = studentDto.LastName;
            student.GroupId = group.Id;
            student.Update();

            var result = await unitOfWork.Students.UpdateAsync(student);

            await unitOfWork.SaveChangesAsync();

            response.Data = result;

            return response;


        }
    }
}
