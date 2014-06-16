using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    public class CustomerController : Controller
    {
        private Northwind db = new Northwind();

        private int pageSize = 10;

        public ActionResult Index(int page = 1)
        {
            var result = GetPagedList(page, pageSize);
            return View(result);
        }

        private IPagedList<Customer> GetPagedList(int page, int pageSize)
        {
            page = page < 1 ? 1 : page;

            var query = db.Customers.OrderBy(x => x.CustomerID);

            return query.ToPagedList(page, pageSize);
        }
    }
}