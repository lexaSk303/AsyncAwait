using AsyncMonitor;

var service = new CheckService();
var checks = new (string Name, TimeSpan Delay)[]
{
    ("Сеть", TimeSpan.FromMilliseconds(700)),
    ("База данных", TimeSpan.FromMilliseconds(900)),
    ("Файлы", TimeSpan.FromMilliseconds(500))
};

while (true)
{
    Console.WriteLine("\n1 — одна проверка; 2 — последовательно; 3 — параллельно; 4 — долгая проверка с отменой; 0 — выход");
    Console.Write("Выберите действие: ");
    var cmd = Console.ReadLine()?.Trim();

    if (cmd == "0")
        break;

    try
    {
        if (cmd == "1")
        {
            // TODO 8: Вызвать метод с await, не применять .Wait() или .Result
            var result = await service.CheckServiceAsync("Одиночный тест", TimeSpan.FromMilliseconds(300));
            Print(new[] { result });
        }
        else if (cmd == "2")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunSequentialAsync(checks);
            sw.Stop();
            Console.WriteLine($"\nПоследовательно завершено за: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "3")
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var r = await service.RunParallelAsync(checks);
            sw.Stop();
            Console.WriteLine($"\nПараллельно завершено за: {sw.ElapsedMilliseconds} мс");
            Print(r);
        }
        else if (cmd == "4")
        {
            // TODO 10: Демонстрация того, что Task.Delay не блокирует поток
            using var cts = new CancellationTokenSource();
            var cancelTask = Task.Run(() =>
            {
                Console.WriteLine("Нажмите любую клавишу для отмены долгой проверки...");
                Console.ReadKey();
                cts.Cancel();
            });

            try
            {
                Console.WriteLine("Запуск долгой проверки (5 секунд)...");
                var result = await service.CheckServiceAsync("Долгий сервис", TimeSpan.FromSeconds(5), cts.Token);
                Print(new[] { result });
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Проверка успешно отменена пользователем! Поток не был заблокирован, мы смогли нажать клавишу.");
            }
        }
        else
        {
            Console.WriteLine("Неизвестная команда.");
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Проверка отменена (OperationCanceledException).");
    }
    catch (Exception ex)
    {
        // TODO 9: Не скрывать исключение; сообщить тип и причину. Никаких пустых catch!
        Console.WriteLine($"\n[ОШИБКА] Тип: {ex.GetType().Name}");
        Console.WriteLine($"Причина: {ex.Message}");
    }
}

static void Print(IEnumerable<CheckResult> xs)
{
    Console.WriteLine("\n--- Результаты ---");
    foreach (var x in xs)
    {
        string status = x.Success ? "OK" : "FAIL";
        Console.WriteLine($"{status} | {x.Name,-15} | Время: {x.Duration.TotalMilliseconds,4:F0} мс | {x.Message}");
    }
    Console.WriteLine("----------------");
}