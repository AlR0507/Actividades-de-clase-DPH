using Humanizer;
using Microsoft.AspNetCore.Mvc;
using NumericSeries.Models;
using System.Collections.Generic;

namespace NumericSeries.Controllers
{
    public class SeriesController : Controller
    {
        [HttpGet("/series/{series}/{n:min(0)}")]
        public IActionResult Index(string series, int n = 0)
        {
            List<int> result = null;
            string message = null;

            switch (series)
            {
                case "natural":
                    result = GetNaturalSeries(n + 1);
                    break;
                case "fibonacci":
                    result = GetFibonacciSeries(n + 1);
                    break;
                case  "cuadratic":
                    result = GetCuadraticSeries(n + 1);
                    break;
                case "cubic":
                    result = GetCubicSeries(n + 1);
                    break;
                case "even":
                    result = GetEvenSeries(n + 1);
                    break;
            }



            return View(new SeriesViewModel()
            {
                Series = series.ApplyCase(LetterCasing.Sentence),
                N = n,
                Result = result,
                Message = message
            }); 
        }
        private List<int> GetNaturalSeries(int n)
        {
            var series = new List<int>();
            for (int i = 1; i <= n; i++)
            {
                series.Add(i);
            }
            return series;
        }
        private List<int> GetFibonacciSeries(int n)
        {
            var series = new List<int>();
            int a = 0, b = 1;
            for (int i = 0; i <= n; i++)
            {
                series.Add(a);
                int temp = a + b;
                a = b;
                b = temp;
            }
            return series;
        }
        private List<int> GetCuadraticSeries(int n)
        {
            var series = new List<int>();
            for (int i = 1; i <= n; i++)
            {
                series.Add(i * i);
            }
            return series;
        }
        private List<int> GetCubicSeries(int n)
        {
            var series = new List<int>();
            for (int i = 1; i <= n; i++)
            {
                series.Add(i * i * i);
            }
            return series;
        }
        private List<int> GetEvenSeries(int n)
        {
            var series = new List<int>();
            for (int i = 1; i <= n; i++)
            {
                series.Add(i * 2);
            }
            return series;
        }

    }
}
