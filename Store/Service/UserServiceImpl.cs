using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Store.Data;
using Store.Models;

namespace Store.Service
{
        public class UserServiceImpl : UserService
        {
            private StoreContext db;
            public UserServiceImpl(StoreContext _db)
            {
                db = _db;
            }
            public bool Create(User user)
            {
                try
                {
                    if (db.User.Any(a => a.UserEmail == user.UserEmail))
                    {
                        Console.WriteLine("Email already exists.");
                        return false;
                    }

                    db.User.Add(user);
                    var result = db.SaveChanges() > 0;
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error creating user: " + ex.Message);
                    return false;
                }
            }

            public User findByEmail(string email)
            {
                return db.User.AsNoTracking().SingleOrDefault(a => a.UserEmail == email);
            }

            public bool Login(string useremail, string userpassword)
            {
                var user = db.User.SingleOrDefault(a => a.UserEmail == useremail);
                if (user != null)
                {
                    return BCrypt.Net.BCrypt.Verify(userpassword, user.UserPassword);
                }
                return false;
            }

            public bool Update(User user)
            {
                try
                {
                    db.Entry(user).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    return db.SaveChanges() > 0;
                }
                catch
                {
                    return false;
                }
            }
        }
    }

