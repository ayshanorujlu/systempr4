using Bogus;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using SystemProgramming4.Commands;
using SystemProgramming4.Models;

namespace SystemProgramming4.ViewModels
{
    public class MainViewModel : DependencyObject
    {
        public Dispatcher DispatcherC { get; set; } = Dispatcher.CurrentDispatcher;
        public CancellationTokenSource CancellationToken { get; set; } = new();

        public RelayCommand StartOperationCommand { get; set; }
        public RelayCommand EndOperationCommand { get; set; }

        public const int Carcount = 300;


        public ObservableCollection<Car> Cars
        {
            get { return (ObservableCollection<Car>)GetValue(CarsProperty); }
            set { SetValue(CarsProperty, value); }
        }


        public static readonly DependencyProperty CarsProperty =
            DependencyProperty.Register("Cars", typeof(ObservableCollection<Car>), typeof(MainViewModel));



        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Time.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(MainViewModel));

        public Stopwatch StopWatch { get; set; } = new();
        public bool IsMultiTreadOperation { get; set; } = false;
        public bool StartThread { get; set; } = false;

        static MainViewModel()
        {
            if (Directory.Exists("Cars"))
            {
                if (Directory.GetFiles($@"{Environment.CurrentDirectory}\Cars").Length == Carcount)
                    return;
                else 
                {
                    foreach (var item in Directory.GetFiles($@"{Environment.CurrentDirectory}\Cars"))
                        File.Delete(item);
                }
            }
            if (!Directory.Exists("Cars"))
                Directory.CreateDirectory("Cars");

            var CarGenerator = new Faker<Car>().RuleFor(car => car.Model, fake => fake.Vehicle.Model())
                                               .RuleFor(car => car.Vendor, fake => fake.Vehicle.Manufacturer())
                                               .RuleFor(car => car.Year, fake => fake.Random.Number(1970, 2010))
                                               .RuleFor(car => car.ImagePath, fake => fake.Image.Transport());

            foreach (var item in CarGenerator.Generate(Carcount))
            { 
                var json = JsonConvert.SerializeObject(item, Formatting.Indented);
                File.WriteAllText($@"{Environment.CurrentDirectory}\Cars\{item.Id}.json", json);

            }

        }

        public MainViewModel()
        {
            StartOperationCommand = new(
                execute_: (sender) => StartOperation(), canExecute_: (sender) => !StartThread);
            EndOperationCommand = new((sender) => CancellationToken.Cancel(), (sender) => StartThread);
            Cars = new();
            Time = "00:00:00";
        }

        private void StartOperation()
        {

            Cars.Clear();
            Time = "00:00:00";
            StartThread = true;
            StopWatch.Start();

            if (IsMultiTreadOperation)
            {
                foreach (var item in Directory.GetFiles($@"{Environment.CurrentDirectory}\Cars"))
                {
                    ThreadPool.QueueUserWorkItem(MultiThreadOperation, item);
                }
            }
            else
            {
                ThreadPool.QueueUserWorkItem(SingleThreadOperation);
            }

        }

        public void MultiThreadOperation(object? path)
        {
            lock (DispatcherC.Invoke(() => Cars))
            {
                var json = File.ReadAllText(path.ToString());
                var car = JsonConvert.DeserializeObject<Car>(json);

                if (CancellationToken.Token.IsCancellationRequested)
                {
                    DispatcherC.Invoke(() => Cars.Clear());
                    Reset();
                    return;
                }
                if (car is not null)
                {
                    DispatcherC.Invoke(() => Cars.Add(car));
                }
            }
            if (DispatcherC.Invoke(() => Cars).Count == Carcount)
            {
                Reset();
            }
            
        }
        
        public void SingleThreadOperation(object? path)
        {
            foreach (var item in Directory.GetFiles($@"{Environment.CurrentDirectory}\Cars"))
            {
                if (item.EndsWith(".json"))
                {
                    var json = File.ReadAllText(item);
                    var car = JsonConvert.DeserializeObject<Car>(json);

                    if (CancellationToken.Token.IsCancellationRequested)
                    {
                        DispatcherC.Invoke(() => Cars.Clear());
                        Reset();
                        return;
                    }
                    if (car is not null)
                    {
                        DispatcherC.Invoke(() => Cars.Add(car));
                    }
                }
                if (CancellationToken.Token.IsCancellationRequested)
                {
                    DispatcherC.Invoke(() => Cars.Clear());
                    Reset();
                    return;
                }
            }
            Reset();
        }

        public void Reset()
        {
            StartThread = false;
            StopWatch.Stop();
            CancellationToken = new();

            DispatcherC.Invoke(() => Time = TimeSpan.FromMicroseconds(StopWatch.ElapsedMilliseconds).ToString());

            StopWatch = new();
        }

        
    }
}
