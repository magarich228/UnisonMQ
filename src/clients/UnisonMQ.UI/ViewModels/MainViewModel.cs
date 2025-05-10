using System;
using System.Net.Http;
using CommunityToolkit.Mvvm.ComponentModel;
using PromQL.Parser;

namespace UnisonMQ.UI.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "UnisonMQ";

    public MainViewModel()
    {
        // using HttpClient client = new HttpClient()
        // {
        //     BaseAddress = new Uri("http://localhost:15888"),
        // };
        //
        // var metricsResponse = client.GetAsync("metrics").Result;
        //
        // metricsResponse.EnsureSuccessStatusCode();
        // var metricsContent = metricsResponse.Content.ReadAsStringAsync().Result;
        //
        // Parser.
    }
}
