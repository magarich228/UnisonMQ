open System
open System.Collections.Generic
open System.Diagnostics
open NUnit.Engine
open NUnit.Framework.Api

[<EntryPoint>]
let main argv =
    try
        if argv.Length < 3 then
            failwithf "Использование: %s <потоки> <фильтр> <время_выполнения>" 
                (Process.GetCurrentProcess().ProcessName)

        let workers = argv[0].ToString() |> int
        let filter = argv[1]
        let runTime = TimeSpan.Parse(argv[2])

        let settings = new Dictionary<string, Object>()
        settings.Add("NumberOfTestWorkers", workers)
        settings.Add("DefaultTimeout", 30000) // 30 секунд таймаут по умолчанию

        let runner = new NUnitTestAssemblyRunner(new DefaultTestAssemblyBuilder())
        let assembly = typeof<UnisonMQ.Performance.PerformanceTests>.Assembly
        
        runner.Load(assembly, settings) |> ignore
        
        let filterBuilder = TestFilterBuilder()
        filterBuilder.SelectWhere(filter)
        
        let testFilter = filterBuilder.GetFilter()
        let frameworkFilter = NUnit.Framework.Internal.TestFilter.FromXml(testFilter.Text)
        
        let sw = Stopwatch.StartNew()
        let mutable iterations = 0
        let mutable totalPassed = 0
        let mutable totalFailed = 0

        printfn "Запуск нагрузочного тестирования..."
        printfn $"Параметры: Потоков - {workers}, Фильтр - '{filter}', Время - {runTime}\n"
        
        printfn $"Test filter: {testFilter.Text}"
        printfn $"Framework filter: {frameworkFilter.ToXml(true).OuterXml}"
        
        while sw.Elapsed < runTime do
            let result = runner.Run(null, frameworkFilter)

            iterations <- iterations + 1
            totalPassed <- totalPassed + result.PassCount
            totalFailed <- totalFailed + result.FailCount

            // printfn $"""
            // Итерация #{iterations} [{DateTime.Now}]
            // Успешно: {result.PassCount}
            // Ошибки:  {result.FailCount}
            // Время:   {result.Duration} мс"""

        printfn $"""
        ======== ФИНАЛЬНЫЙ ОТЧЕТ ========
        Всего итераций:      {iterations}
        Общее время:        {sw.Elapsed}
        Успешных тестов:    {totalPassed}
        Ошибочных тестов:   {totalFailed}
        Пропускная способность: {float totalPassed / sw.Elapsed.TotalSeconds:N1} тест/сек
        ==================================
        """
        
        0
    with
    | :? FormatException ->
        Console.ForegroundColor <- ConsoleColor.Red
        printfn "Ошибка формата времени! Используйте формат ЧЧ:ММ:СС"
        Console.ResetColor()
        2
    | ex ->
        Console.ForegroundColor <- ConsoleColor.Red
        printfn $"КРИТИЧЕСКАЯ ОШИБКА: {ex}"
        Console.ResetColor()
        1