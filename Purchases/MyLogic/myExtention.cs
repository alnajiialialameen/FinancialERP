using Purchases.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace Purchases.MyLogic
{
    public class myExtention
    {
        static Entities db = new Entities();
        //upload file
        public static string UploadFile(int? Id, string Title)
        {
            //check if there are 
            string root = HttpContext.Current.Server.MapPath("~/Uploads");
            string destinationPath = root + "/" + Id;
            string fileName = "";
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            if (HttpContext.Current.Request.Files.Count > 0)
            {
                for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                {
                    var file = HttpContext.Current.Request.Files[i];

                    fileName = Title + Path.GetExtension(file.FileName);

                    var path = Path.Combine(destinationPath, fileName);

                    if (!File.Exists(Path.Combine(destinationPath, fileName)))
                    {
                        file.SaveAs(path);
                    }
                    else
                    {
                        File.Delete(Path.Combine(destinationPath, fileName));
                        file.SaveAs(path);
                    }

                    fileName = "Uploads/" + Id + "/" + fileName;
                }
            }

            return fileName;
        }

        //not workk
        public static List<string> UploadFiles(int Id)
        {
            List<string> imagesPathes = new List<string>();
            //check if there are 
            string root = HttpContext.Current.Server.MapPath("~/Images/ProfilesImage");
            string destinationPath = root + "/" + Id;
            string fileName = "";
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            if (HttpContext.Current.Request.Files.Count > 0)
            {
                for (int i = 0; i < HttpContext.Current.Request.Files.Count; i++)
                {
                    var file = HttpContext.Current.Request.Files[i];

                    fileName = "Image" + (i + 1).ToString() + Path.GetExtension(file.FileName);

                    var path = Path.Combine(destinationPath, fileName);

                    if (!File.Exists(Path.Combine(destinationPath, fileName)))
                    {
                        file.SaveAs(path);
                    }
                    else
                    {
                        File.Delete(Path.Combine(destinationPath, fileName));
                        file.SaveAs(path);
                    }

                    fileName = "Images/ProfilesImage/Image" + (i + 1).ToString() + Path.GetExtension(file.FileName);
                    imagesPathes.Add(fileName);
                }
            }

            return imagesPathes;
        }

        //upload pdf file
        public static int DeleteUploadedFile(int? Id, string Title)
        {
            try
            {
                //check if there are 
                string root = HttpContext.Current.Server.MapPath("~/Uploads");
                string destinationPath = root + "/" + Id;

                if (File.Exists(Path.Combine(destinationPath, Title)))
                {
                    File.Delete(Path.Combine(destinationPath, Title));
                    return 1;
                }
                else
                {
                    return -1;
                } 
            }
            catch(Exception e)
            {
                return -1;
            }
        }

        /*---------------------------------------------------------*/
        //not work
        public static string UploadAnnouncementsImages(int Id, HttpPostedFile file, int i)
        {
            //check if there are 
            string root = HttpContext.Current.Server.MapPath("~/Images/AnnouncementsImages");
            string destinationPath = root + "/" + Id;

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            string fileName = "Image" + (i + 1).ToString() + Path.GetExtension(file.FileName);
            var path = Path.Combine(destinationPath, fileName);

            file.SaveAs(path);

            fileName = "Images/AnnouncementsImages/" + fileName;

            return fileName;
        }

        public static int UpdateActualExchange(int BalanceId, decimal oldAmount, decimal newAmount, int type,  string userid)
        {
            try
            {
                if (BalanceId > 0)
                {
                    var balanceObj = db.Balances.Find(BalanceId);

                    if (type > 0)
                    {
                        balanceObj.ActualExchange = balanceObj.ActualExchange - oldAmount + newAmount;
                    }
                    else
                    {
                        balanceObj.ActualExchange = balanceObj.ActualExchange - oldAmount - newAmount;
                    }

                    balanceObj.RelativeDeviation = balanceObj.Credint - balanceObj.ActualExchange;
                    balanceObj.DeviationRatio =  balanceObj.ActualExchange > 0? Convert.ToDecimal((balanceObj.RelativeDeviation / balanceObj.Credint)) / 100 : 0;
                    balanceObj.UpdatedBy = userid;
                    balanceObj.UpdatingDate = DateTime.Now;

                    db.Entry(balanceObj).State = EntityState.Modified;
                    db.SaveChanges();

                    return 1;
                }
                else
                {
                    return 0;
                }
            }catch(Exception e)
            {
                return -1;
            }
        }

        public static int UpdateActualExchange(int BalanceId, decimal oldAmount, decimal newAmount, string userid)
        {
            try
            {
                if (BalanceId > 0)
                {
                    var balanceObj = db.Balances.Find(BalanceId);

                    balanceObj.ActualExchange = balanceObj.ActualExchange - oldAmount + newAmount;
                    balanceObj.RelativeDeviation = balanceObj.Credint - balanceObj.ActualExchange;
                    balanceObj.DeviationRatio = balanceObj.ActualExchange > 0 ? Convert.ToDecimal((balanceObj.RelativeDeviation / balanceObj.Credint)) / 100 : 0;
                    balanceObj.UpdatedBy = userid;
                    balanceObj.UpdatingDate = DateTime.Now;

                    db.Entry(balanceObj).State = EntityState.Modified;
                    db.SaveChanges();

                    return 1;
                }
                else
                {
                    return 0;
                }
            }
            catch (Exception e)
            {
                return -1;
            }
        }
    }
}