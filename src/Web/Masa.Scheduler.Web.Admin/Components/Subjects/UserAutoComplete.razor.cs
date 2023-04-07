// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.Subjects;

public partial class UserAutoComplete : ProComponentBase
{
    [Inject]
    public IAutoCompleteClient AutoCompleteClient { get; set; } = default!;

    [Inject]
    public IAuthClient AuthClient { get; set; } = default!;

    [Parameter]
    public List<Guid> Value { get; set; } = new();

    [Parameter]
    public EventCallback<List<Guid>> ValueChanged { get; set; }

    [Parameter]
    public string Label { get; set; } = "";

    [Parameter]
    public bool Readonly { get; set; }

    [Parameter]
    public int Page { get; set; } = 1;

    [Parameter]
    public int PageSize { get; set; } = 10;

    bool _loading;

    protected List<UserSelectModel> Users { get; set; } = new();

    public string Search { get; set; } = "";

    protected override async Task OnParametersSetAsync()
    {
        if (!Users.Any())
        {
            await InitUsers();
        }

        base.OnParametersSet();
    }

    private async Task InitUsers()
    {
        var list = await AuthClient.UserService.GetListByIdsAsync(Value.ToArray());
        Users = list.Select(x => new UserSelectModel(x.Id, x.Name ?? string.Empty, x.DisplayName, x.Account, x.PhoneNumber ?? string.Empty, x.Email ?? string.Empty, x.Avatar)).ToList();
        StateHasChanged();
    }

    private async Task Remove(UserSelectModel staff)
    {
        if (Readonly is false)
        {
            var value = new List<Guid>();
            value.AddRange(Value);
            value.Remove(staff.Id);
            await ValueChanged.InvokeAsync(value);
        }
    }

    private bool FilterItem(UserSelectModel item, string queryText, string itemText)
    {
        return item.DisplayName.Contains(queryText);
    }

    private async Task QuerySelection(string search)
    {
        search = search.TrimStart(' ').TrimEnd(' ');
        Search = search;
        await Task.Delay(300);
        if (search != Search)
        {
            return;
        }

        _loading = true;
        var response = await AutoCompleteClient.GetBySpecifyDocumentAsync<UserSelectModel>(search, new AutoCompleteOptions
        {
            Page = Page,
            PageSize = PageSize,
        });
        var users = response.Data;
        var seletedItems = Users.Where(x => Value.Contains(x.Id)).ToList();
        Users = seletedItems.UnionBy(users, user => user.Id).ToList();
        _loading = false;
    }

    public string TextView(UserSelectModel user)
    {
        if (!string.IsNullOrEmpty(user.DisplayName)) return user.DisplayName;
        if (!string.IsNullOrEmpty(user.Name)) return user.Name;
        if (!string.IsNullOrEmpty(user.PhoneNumber)) return user.PhoneNumber;
        return "";
    }
}