using Newtonsoft.Json.Linq;
using Purchases.Models;
using Purchases.Models.ViewModal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Purchases.MyLogic
{
    public class ConsumeHRAPI
    {
        //جلب بيانات الادارات
        public async Task<List<deptVM>> getDepartments()
        {
            try
            {
                //http://localhost:57677/api/
                List<deptVM> department = new List<deptVM>();
                HttpClient HC = new HttpClient();
                HC.BaseAddress = new Uri(Constant.BaseAPIURL);
                var ConsumeApi = HC.GetAsync("Departments");
                ConsumeApi.Wait();
                var response = ConsumeApi.Result;
                
                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var jsonArray = JArray.Parse(jsonString); // لو رجع مصفوفة JSON

                    department = jsonArray.Select(p => new deptVM
                    {
                        id = (int)p["id"],   // الحقل "id" بالحروف الصغيرة
                        text = (string)p["name"] // الحقل "name" بالحروف الصغيرة
                    }).ToList();
                }

                return department;
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
                HC.BaseAddress = new Uri(Constant.BaseAPIURLServer);
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

        //جلب بيانات البنك لجميع الموظفين
        public List<EmployeeBankInfoDTO> getEmployeesBankInfo()
        {
            try
            {
                //http://localhost:57677/api/
                List<EmployeeBankInfoDTO> EmployeesList = null;
                HttpClient HC = new HttpClient();
                HC.BaseAddress = new Uri(Constant.BaseAPIURLServer);
                var ConsumeApi = HC.GetAsync("Employees/EmployeesBankInfo");
                ConsumeApi.Wait();
                var ReadData = ConsumeApi.Result;
                if (ReadData.IsSuccessStatusCode)
                {
                    var DisplayRecord = ReadData.Content.ReadAsAsync<List<EmployeeBankInfoDTO>>();
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