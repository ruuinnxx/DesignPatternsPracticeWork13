using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBookingSystem
{
    public enum UserRole
    {
        Guest,
        User,
        Admin
    }

    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public DateTime Date { get; set; }
        public string Location { get; set; } = "";

        public override string ToString()
        {
            return $"[{Id}] {Title} | {Date:dd.MM.yyyy HH:mm} | {Location}";
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public UserRole Role { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {Name} ({Role})";
        }
    }

    public class Booking
    {
        public int Id { get; set; }
        public User User { get; set; } = null!;
        public Event Event { get; set; } = null!;
        public string Status { get; set; } = "Active"; // Active / Cancelled

        public override string ToString()
        {
            return $"[{Id}] {User.Name} -> {Event.Title} | Status: {Status}";
        }
    }

    public class BookingSystem
    {
        private readonly List<Event> _events = new();
        private readonly List<User> _users = new();
        private readonly List<Booking> _bookings = new();
        private int _nextEventId = 1;
        private int _nextBookingId = 1;

        public BookingSystem()
        {
            SeedTestData();
        }

        private void SeedTestData()
        {
            //jhsefisknfs
            _events.Add(new Event
            {
                Id = _nextEventId++,
                Title = "Конференция по FinTech",
                Date = DateTime.Now.AddDays(7),
                Location = "Astana IT University"
            });
            _events.Add(new Event
            {
                Id = _nextEventId++,
                Title = "Концерт симфонического оркестра",
                Date = DateTime.Now.AddDays(14),
                Location = "Городской театр"
            });
            _events.Add(new Event
            {
                Id = _nextEventId++,
                Title = "Хакатон по ИИ",
                Date = DateTime.Now.AddDays(3),
                Location = "Tech Hub"
            });

            
            _users.Add(new User { Id = 1, Name = "Гость", Role = UserRole.Guest });
            _users.Add(new User { Id = 2, Name = "Иван", Role = UserRole.User });
            _users.Add(new User { Id = 3, Name = "Админ", Role = UserRole.Admin });
        }

        public User? Login()
        {
            Console.WriteLine("Выберите пользователя для входа:");
            foreach (var u in _users)
            {
                Console.WriteLine(u);
            }

            Console.Write("ID пользователя: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var user = _users.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    Console.WriteLine($"Вы вошли как: {user.Name} ({user.Role})\n");
                    return user;
                }
            }

            Console.WriteLine("Пользователь не найден.\n");
            return null;
        }


        public void ShowEvents()
        {
            Console.WriteLine("\nСписок мероприятий:");
            if (_events.Count == 0)
            {
                Console.WriteLine("Нет доступных мероприятий.\n");
                return;
            }

            foreach (var e in _events.OrderBy(e => e.Date))
            {
                Console.WriteLine(e);
            }
            Console.WriteLine();
        }

        public void BookEvent(User user)
        {
            if (user.Role == UserRole.Guest)
            {
                Console.WriteLine("Гость не может бронировать мероприятия. Зарегистрируйтесь.\n");
                return;
            }

            ShowEvents();
            Console.Write("Введите ID мероприятия для бронирования: ");
            if (!int.TryParse(Console.ReadLine(), out int eventId))
            {
                Console.WriteLine("Неверный ID.\n");
                return;
            }

            var ev = _events.FirstOrDefault(e => e.Id == eventId);
            if (ev == null)
            {
                Console.WriteLine("Мероприятие не найдено.\n");
                return;
            }

            var booking = new Booking
            {
                Id = _nextBookingId++,
                User = user,
                Event = ev,
                Status = "Active"
            };
            _bookings.Add(booking);
            Console.WriteLine($"Бронирование создано: {booking}\n");
        }

        public void CancelBooking(User user)
        {
            if (user.Role == UserRole.Guest)
            {
                Console.WriteLine("Гость не может отменять бронирования.\n");
                return;
            }

            var userBookings = _bookings
                .Where(b => b.User.Id == user.Id && b.Status == "Active")
                .ToList();

            if (!userBookings.Any())
            {
                Console.WriteLine("У вас нет активных бронирований.\n");
                return;
            }

            Console.WriteLine("\nВаши активные бронирования:");
            foreach (var b in userBookings)
            {
                Console.WriteLine(b);
            }

            Console.Write("Введите ID бронирования для отмены: ");
            if (!int.TryParse(Console.ReadLine(), out int bookingId))
            {
                Console.WriteLine("Неверный ID.\n");
                return;
            }

            var booking = userBookings.FirstOrDefault(b => b.Id == bookingId);
            if (booking == null)
            {
                Console.WriteLine("Бронирование не найдено.\n");
                return;
            }

            booking.Status = "Cancelled";
            Console.WriteLine("Бронирование отменено.\n");
        }


        private bool IsAdmin(User user)
        {
            if (user.Role != UserRole.Admin)
            {
                Console.WriteLine("Доступ запрещён: требуется роль Администратор.\n");
                return false;
            }
            return true;
        }

        public void AddEvent(User user)
        {
            if (!IsAdmin(user)) return;

            Console.Write("Название: ");
            string? title = Console.ReadLine();
            Console.Write("Дата (дд.мм.гггг чч:мм): ");
            string? dateStr = Console.ReadLine();
            Console.Write("Место: ");
            string? location = Console.ReadLine();

            if (!DateTime.TryParse(dateStr, out DateTime date))
            {
                Console.WriteLine("Неверный формат даты.\n");
                return;
            }

            var ev = new Event
            {
                Id = _nextEventId++,
                Title = title ?? "Без названия",
                Date = date,
                Location = location ?? "Не указано"
            };
            _events.Add(ev);
            Console.WriteLine($"Мероприятие добавлено: {ev}\n");
        }

        public void EditEvent(User user)
        {
            if (!IsAdmin(user)) return;

            ShowEvents();
            Console.Write("Введите ID мероприятия для редактирования: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Неверный ID.\n");
                return;
            }

            var ev = _events.FirstOrDefault(e => e.Id == id);
            if (ev == null)
            {
                Console.WriteLine("Мероприятие не найдено.\n");
                return;
            }

            Console.Write($"Новое название ({ev.Title}): ");
            string? title = Console.ReadLine();
            Console.Write($"Новая дата ({ev.Date:dd.MM.yyyy HH:mm}): ");
            string? dateStr = Console.ReadLine();
            Console.Write($"Новое место ({ev.Location}): ");
            string? location = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(title))
                ev.Title = title;
            if (!string.IsNullOrWhiteSpace(dateStr) && DateTime.TryParse(dateStr, out DateTime newDate))
                ev.Date = newDate;
            if (!string.IsNullOrWhiteSpace(location))
                ev.Location = location;

            Console.WriteLine("Мероприятие обновлено.\n");
        }

        public void DeleteEvent(User user)
        {
            if (!IsAdmin(user)) return;

            ShowEvents();
            Console.Write("Введите ID мероприятия для удаления: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Неверный ID.\n");
                return;
            }

            var ev = _events.FirstOrDefault(e => e.Id == id);
            if (ev == null)
            {
                Console.WriteLine("Мероприятие не найдено.\n");
                return;
            }

            _events.Remove(ev);
            Console.WriteLine("Мероприятие удалено.\n");
        }

        public void ViewAllBookings(User user)
        {
            if (!IsAdmin(user)) return;

            Console.WriteLine("\nВсе бронирования:");
            if (_bookings.Count == 0)
            {
                Console.WriteLine("Бронирований нет.\n");
                return;
            }

            foreach (var b in _bookings.OrderBy(b => b.Id))
            {
                Console.WriteLine(b);
            }
            Console.WriteLine();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var system = new BookingSystem();

            while (true)
            {
                var currentUser = system.Login();
                if (currentUser == null)
                {
                    continue;
                }

                bool exitUser = false;
                while (!exitUser)
                {
                    Console.WriteLine("Меню:");
                    Console.WriteLine("1. Просмотр мероприятий");
                    Console.WriteLine("2. Бронирование мероприятия");
                    Console.WriteLine("3. Отмена бронирования");
                    Console.WriteLine("4. (Админ) Добавить мероприятие");
                    Console.WriteLine("5. (Админ) Редактировать мероприятие");
                    Console.WriteLine("6. (Админ) Удалить мероприятие");
                    Console.WriteLine("7. (Админ) Просмотр всех бронирований");
                    Console.WriteLine("0. Сменить пользователя / Выход к выбору пользователя");
                    Console.Write("Выбор: ");

                    string? input = Console.ReadLine();
                    Console.WriteLine();

                    switch (input)
                    {
                        case "1":
                            system.ShowEvents();
                            break;
                        case "2":
                            system.BookEvent(currentUser);
                            break;
                        case "3":
                            system.CancelBooking(currentUser);
                            break;
                        case "4":
                            system.AddEvent(currentUser);
                            break;
                        case "5":
                            system.EditEvent(currentUser);
                            break;
                        case "6":
                            system.DeleteEvent(currentUser);
                            break;
                        case "7":
                            system.ViewAllBookings(currentUser);
                            break;
                        case "0":
                            exitUser = true;
                            break;
                        default:
                            Console.WriteLine("Неверный выбор.\n");
                            break;
                    }
                }
            }
        }
    }
}