using Purchases.Models;
using Purchases.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace Purchases.MyLogic
{
    public class EmployeePettyCashCustodyObj
    {
        private Entities db = new Entities();
        private SharedClass sh = new SharedClass();
        //private ConsumeHRAPI hrAPI = new ConsumeHRAPI();

        //public List<EmployeePettyCashCustodyVM> LoadData(string userId, int accTreeId = 0)
        //{
        //    var financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

        //    var EmpRec = db.Recipients.Where(q => q.EmployeeId > 0).ToList();

        //    var data = db.EmployeePettyCashCustodies
        //                                .Where(q => q.FinancialCycleId == financialCycleId && (accTreeId <= 0 || q.AccTreeId == accTreeId))
        //                                .Select(q => new EmployeePettyCashCustodyVM()
        //                                {
        //                                    AccTreeId = q.AccTreeId,
        //                                    AccTreeName = q.AccountTree.AccName,
        //                                    EmployeeName = EmpRec.FirstOrDefault(p => p.EmployeeId == q.EmployeeId).RecipientName ?? "",
        //                                    EmployeeId = q.EmployeeId,
        //                                    Id = q.Id,
        //                                    PaiedAmount = q.PaiedAmount??0,
        //                                    TotalDeductionAmount = q.TotalDeductionAmount
        //                                }).ToList();

        //    return data;
        //}

        public List<EmployeePettyCashCustodyVM> LoadData(string userId, int accTreeId = 0)
        {
            var financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            var data = (
                from rec in db.Recipients
                where rec.EmployeeId > 0

                join q in db.EmployeePettyCashCustodies
                    .Where(x => x.FinancialCycleId == financialCycleId
                             && (accTreeId <= 0 || x.AccTreeId == accTreeId))
                //&& (x.PaiedAmount ?? 0) > 0)
                on rec.EmployeeId equals q.EmployeeId into custodies

                from q in custodies.DefaultIfEmpty()

                select new EmployeePettyCashCustodyVM
                {
                    EmployeeId = rec.EmployeeId,
                    EmployeeName = rec.RecipientName,

                    Id = q == null ? 0 : q.Id,
                    AccTreeId = q == null ? 0 : q.AccTreeId,
                    AccTreeName = q == null ? "" : q.AccountTree.AccName,

                    PaiedAmount = q == null ? 0 : (q.PaiedAmount ?? 0),
                    TotalDeductionAmount = q == null
                        ? 0
                        : q.TotalDeductionAmount
                }
            ).ToList();

            return data;
        }

        public List<DDListObj> GetPettyCachItems()
        {

            var data = db.AccountTreeHrDeductions
            .Select(q => new DDListObj
            {
                id = q.AccTreeId ?? 0,
                text = q.AccountTree.AccName
            })
            .ToList();

            return data;
        }

        // جبارة الله مبلول محمد زين

        public int SaveData(List<EmployeePettyCashCustodyVM> data, string userId)
        {
            var listToSave = new List<EmployeePettyCashCustody>();
            int financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            foreach (var item in data)
            {
                var obj = new EmployeePettyCashCustody()
                {
                    EmployeeId = item.EmployeeId,
                    TotalDeductionAmount = item.TotalDeductionAmount,
                    PaiedAmount = item.PaiedAmount,
                    AccTreeId = item.AccTreeId,
                    FinancialCycleId = financialCycleId,
                };

                listToSave.Add(obj);
            }

            try
            {
                db.EmployeePettyCashCustodies.AddRange(listToSave);
                db.SaveChanges();

                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public int SaveDataSingleRow(EmployeePettyCashCustodyVM item, string userId)
        {
            int financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            var obj = new EmployeePettyCashCustody()
            {
                EmployeeId = item.EmployeeId,
                TotalDeductionAmount = item.TotalDeductionAmount,
                PaiedAmount = item.PaiedAmount,
                AccTreeId = item.AccTreeId,
                FinancialCycleId = financialCycleId,
            };

            try
            {
                db.EmployeePettyCashCustodies.Add(obj);
                db.SaveChanges();

                return 1;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        public int UpdateData(EmployeePettyCashCustodyVM item, string userId)
        {
            int financialCycleId = sh.GetUserCurrentFinancialCycleId(userId);

            var obj = new EmployeePettyCashCustody()
            {
                Id = item.Id,
                EmployeeId = item.EmployeeId,
                TotalDeductionAmount = item.TotalDeductionAmount,
                PaiedAmount = item.PaiedAmount,
                AccTreeId = item.AccTreeId,
                FinancialCycleId = financialCycleId,
            };

            try
            {
                db.Entry(obj).State = EntityState.Modified;
                db.SaveChanges();

                return 1;
            }
            catch
            {
                return -1;
            }
        }

    }
}
