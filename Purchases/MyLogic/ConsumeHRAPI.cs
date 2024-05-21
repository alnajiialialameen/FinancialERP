using Purchases.Models;
using Purchases.Models.ViewModal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;


namespace Purchases.MyLogic
{
    public class ConsumeHRAPI
    {
        //جلب بيانات الادارات
        public List<deptVM> getDepartments()
        {
            try
            {
                //http://localhost:57677/api/
                List<DepartmentVM> department = null;
                HttpClient HC = new HttpClient();
                HC.BaseAddress = new Uri(Constant.BaseAPIURL);
                var ConsumeApi = HC.GetAsync("Departments");
                ConsumeApi.Wait();
                var ReadData = ConsumeApi.Result;
                if (ReadData.IsSuccessStatusCode)
                {
                    var DisplayRecord = ReadData.Content.ReadAsAsync<List<DepartmentVM>>();
                    DisplayRecord.Wait();
                    department = DisplayRecord.Result;
                }

                var data = department.Select(p => new
                deptVM
                {
                    id = p.Id,
                    text = p.Name,
                }).ToList();

                return data;
                //return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //جلب بيانات الموظفين
        public List<EmployeeDTO> getAllEmployees()
        {
            try
            {
                //http://localhost:57677/api/
                List<EmployeeDTO> EmployeesList = null;
                HttpClient HC = new HttpClient();
                HC.BaseAddress = new Uri(Constant.BaseAPIURL);
                var ConsumeApi = HC.GetAsync("Employees/AllEmployees");
                ConsumeApi.Wait();
                var ReadData = ConsumeApi.Result;
                if (ReadData.IsSuccessStatusCode)
                {
                    var DisplayRecord = ReadData.Content.ReadAsAsync<List<EmployeeDTO>>();
                    DisplayRecord.Wait();
                    EmployeesList = DisplayRecord.Result;
                }

                return EmployeesList;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}