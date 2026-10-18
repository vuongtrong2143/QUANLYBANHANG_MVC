using System.Linq;
using System.Net;
using System.Web.Mvc;
using QUANLYBANHANG_MVC.Models;

namespace QUANLYBANHANG_MVC.Controllers
{
    public class TrangchuController : Controller
    {
        // Context dùng trong controller này.
        private readonly dbQUANLYBANHANGEntities db = new dbQUANLYBANHANGEntities();

        // GET: Trangchu
        public ActionResult Index()
        {
            var model = new TrangchuViewModel
            {
                DanhMuc = db.tbDANHMUCs.ToList(),
                SanPham = db.tbSANPHAMs.ToList()
            };

            return View(model);
        }

        // GET: Trangchu/Details/5
        public ActionResult Details(int? id)
        {
            // Không có mã sản phẩm trong yêu cầu.
            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Tìm sản phẩm theo khóa chính.
            tbSANPHAM sanPham = db.tbSANPHAMs.Find(id.Value);

            // Không tìm thấy sản phẩm.
            if (sanPham == null)
            {
                return HttpNotFound();
            }

            // Danh mục dùng cho menu bên trái của trang Details.
            ViewBag.DanhMuc = db.tbDANHMUCs.ToList();
            // Truyền một sản phẩm cho Details.cshtml.
            return View(sanPham);
        }

        // Giải phóng context khi controller được giải phóng.
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}