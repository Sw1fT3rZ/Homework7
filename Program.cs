using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace BarberShopSimulation
{
    class BarberShop
    {
        private readonly int waitingRoomSeats;
        private readonly SemaphoreSlim waitingRoomSemaphore;
        private readonly object barberLock = new object();
        private bool barberSleeping = true;
        private int totalCustomers;

        public BarberShop(int seats, int customerCount)
        {
            waitingRoomSeats = seats;
            waitingRoomSemaphore = new SemaphoreSlim(seats); 
            totalCustomers = customerCount;
        }

        public async Task EnterShopAsync(Customer customer)
        {
            Console.WriteLine($"{customer.Name} enters the shop.");
            if (await waitingRoomSemaphore.WaitAsync(0)) 
            {
                Console.WriteLine($"{customer.Name} is waiting in the lounge.");
                await Task.Run(() => BarberWork(customer)); 
            }
            else
            {
                Console.WriteLine($"{customer.Name} leaves as there are no available seats.");
                DecreaseCustomerCount();
            }
        }

        private void BarberWork(Customer customer)
        {
            lock (barberLock)
            {
                if (barberSleeping)
                {
                    Console.WriteLine("The barber wakes up as there is a customer.");
                    barberSleeping = false;
                }

                Console.WriteLine($"The barber starts cutting {customer.Name}'s hair.");
                Thread.Sleep(5000); 
                Console.WriteLine($"The barber has finished cutting {customer.Name}'s hair.");
                waitingRoomSemaphore.Release();
                DecreaseCustomerCount();
            }
        }

        private void DecreaseCustomerCount()
        {
            lock (barberLock)
            {
                totalCustomers--;
                if (totalCustomers <= 0)
                {
                    Console.WriteLine("The barber falls asleep as there are no more customers.");
                    barberSleeping = true;
                }
            }
        }
    }

    class Customer
    {
        public string Name { get; }

        public Customer(string name)
        {
            Name = name;
        }
    }

    class Program
    {
        static async Task Main(string[] args)
        {
            string[] customerNames = { "Alice", "Bob", "Charlie", "Diana", "Ethan", "Fiona" };
            int waitingSeats = 3;
            var barberShop = new BarberShop(waitingSeats, customerNames.Length);

            Random random = new Random();
            List<Task> customerTasks = new List<Task>();

            foreach (var name in customerNames)
            {
                await Task.Delay(random.Next(1000, 2500)); 
                Customer customer = new Customer(name);
                customerTasks.Add(barberShop.EnterShopAsync(customer)); 
            }

            await Task.WhenAll(customerTasks);
            Console.WriteLine("Simulation has ended.");
        }
    }
}
