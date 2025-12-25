using System;
using Microsoft.EntityFrameworkCore;
using Pract15.Models;

namespace Pract15.Services
{
    public class DbService
    {
        private static Pract15Context _instance;
        private static readonly object _lock = new object();

        public static Pract15Context Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new Pract15Context();
                    }
                    return _instance;
                }
            }
        }

        public static void SaveChanges()
        {
            Instance.SaveChanges();
        }
    }
}