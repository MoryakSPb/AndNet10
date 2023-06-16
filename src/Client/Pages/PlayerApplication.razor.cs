using System.Net;
using System.Net.Http.Json;
using System.Text.Encodings.Web;
using AndNet.Manager.Shared.Models;
using Markdig;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace AndNet.Manager.Client.Pages;

public partial class PlayerApplication : ComponentBase, IDisposable
{
    private ValidationMessageStore _validationMessageStore;
    private EditContext? editContext;
    public static MarkdownPipeline? MarkdownPipeline { get; set; }

    [Inject]
    public HttpClient HttpClient { get; set; } = null!;

    [Inject]
    public IJSRuntime JsRuntime { get; set; } = null!;

    public RenderFragment Rules { get; set; }

    public PlayerApplicationRequest Model { get; set; } = new();
    public bool IsSendEnabled { get; set; } = false;
    public string? ResultMessage { get; set; }
    public bool ToIndexAfterSendEnabled { get; set; } = false;

    public void Dispose()
    {
        if (editContext is not null) editContext.OnFieldChanged -= EditContextOnOnFieldChanged;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        //await JsRuntime.InvokeVoidAsync("renderMath");
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        editContext = new(Model);
        _validationMessageStore = new(editContext);
        editContext.OnFieldChanged += EditContextOnOnFieldChanged;

        /*if (MarkdownPipeline is null)
        {
            MarkdownPipelineBuilder pipelineBuilder = new();
            pipelineBuilder = pipelineBuilder
                .UseAdvancedExtensions()
                .UseBootstrap();
            MarkdownPipeline = pipelineBuilder.Build();
        }

        string rulesHtml = Markdown.ToHtml(await HttpClient.GetStringAsync("static/rules.md"), MarkdownPipeline);*/
        string rulesHtml = await HttpClient.GetStringAsync("static/rules.html");
        Rules = builder => builder.AddMarkupContent(0, rulesHtml);
    }

    private async void EditContextOnOnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (editContext is null) return;
        if (e.FieldIdentifier.FieldName == nameof(Model.SteamLink))
        {
            HttpResponseMessage steamResult =
                await HttpClient.GetAsync($"api/Application/steam?url={UrlEncoder.Default.Encode(Model.SteamLink)}");
            if (steamResult.IsSuccessStatusCode)
            {
                Model.SteamId = await steamResult.Content.ReadFromJsonAsync<ulong>();
                _validationMessageStore.Clear(e.FieldIdentifier);
            }
            else
            {
                Model.SteamId = 0;
                _validationMessageStore.Add(e.FieldIdentifier, "Неверная ссылка на профиль Steam");
            }

            StateHasChanged();
        }
        else if (e.FieldIdentifier.FieldName == nameof(Model.DiscordUsername))
        {
            HttpResponseMessage discordResult =
                await HttpClient.GetAsync(
                    $"api/Application/discord?username={UrlEncoder.Default.Encode(Model.DiscordUsername)}");
            if (discordResult.IsSuccessStatusCode)
            {
                Model.DiscordId = await discordResult.Content.ReadFromJsonAsync<ulong>();
                _validationMessageStore.Clear(e.FieldIdentifier);
            }
            else
            {
                Model.DiscordId = 0;
                _validationMessageStore.Add(e.FieldIdentifier, "Неверное имя пользователя Discord");
            }

            StateHasChanged();
        }
    }


    private async void OnSubmit(EditContext context)
    {
        ResultMessage = "Отправка…";
        ToIndexAfterSendEnabled = false;
        StateHasChanged();
        if (!context.Validate()) return;
        HttpResponseMessage result = await HttpClient.PostAsJsonAsync("api/Application", Model);
        ResultMessage = result.StatusCode switch
        {
            HttpStatusCode.OK =>
                "Ваша заявка принята на рассмотрение! Обычно это занимает около суток. О результате вам сообщит наш бот в Discord!",
            HttpStatusCode.NoContent => "Вы уже являетесь участником клана!",
            HttpStatusCode.Conflict => "Похоже, мы уже знакомы с вами. Свяжитесь напрямую с одним из советником.",
            HttpStatusCode.Forbidden => "На данный момент вам запрещено подавать повторную заявку в клан",
            HttpStatusCode.BadRequest => "Заявка заполнена неверно",
            _ => result.ReasonPhrase
        };
        await JsRuntime.InvokeVoidAsync("localStorage.setItem", "Application.IsSent", true);
        ToIndexAfterSendEnabled = true;
        StateHasChanged();
    }
}