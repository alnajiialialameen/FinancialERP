using Microsoft.AspNet.Identity;
using Purchases.MyLogic;
using System.Linq;
using System.Web.Mvc;

namespace Purchases.Controllers
{
    public class DeleteTransactionsController : Controller
    {
        DeleteTransactionObj obj = new DeleteTransactionObj();

        // GET: DeleteTransactions
        public ActionResult Delete(int Id)
        {
            var userid = User.Identity.GetUserId();
            int res = obj.DeleteTransaction(Id, userid);
            
            if(res > 0)
            {
                return Json(new { Message = "تمت عملية الحذف  بنجاح", Title = "نجاح", Status = "success" }, JsonRequestBehavior.AllowGet);
            }else if(res == -100)
            {
                return Json(new { Message = "العملية غير موجودة او تفاصيلها غير مدخلة", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Message = "لم تتم العملية بي نجاح", Title = "خطأ", Status = "error" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult getDeletedDetailData(int Id)
        {
            var data = obj.getDeletedDetailData(Id);

            return Json(data, JsonRequestBehavior.AllowGet);
        }
   
    }
}