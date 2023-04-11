// Copyright (c) MASA Stack All rights reserved.
// Licensed under the Apache License. See LICENSE.txt in the project root for license information.

namespace Masa.Scheduler.Web.Admin.Components.AlarmRules;

public partial class LogAlarmRuleUpsertModal : ProComponentBase
{
    [Parameter]
    public EventCallback<Guid> OnOk { get; set; }

    [Inject]
    public IPmClient PmClient { get; set; } = default!;

    [Inject]
    public ITscClient TscClient { get; set; } = default!;

    [Inject]
    public IAlertClient AlertClient { get; set; } = default!;

    private MForm? _form;
    private AlarmRuleUpsertViewModel _model = new();
    private bool _visible;
    private Guid _entityId;
    private bool _cronVisible;
    private string _tempCron = string.Empty;
    private string _nextRunTimeStr = string.Empty;
    private List<string> _items = new();
    private List<ProjectModel> _projectItems = new();
    private List<AppDetailModel> _appItems = new();
    private List<MappingResponseDto> _fields = new();
    private bool _isChange = false;

    protected override string? PageName { get; set; } = "AlarmRuleBlock";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _projectItems = await PmClient.ProjectService.GetListAsync() ?? new();
        _fields = (await TscClient.LogService.GetMappingAsync()).ToList();
    }

    public async Task OpenModalAsync(Guid entityId, AlarmRuleUpsertViewModel? model = null)
    {
        _entityId = entityId;
        _model = model ?? new();
        _model.Type = AlarmRuleType.Log;

        if (_entityId != default)
        {
            await GetFormDataAsync();
        }
        else
        {
            await HandleProjectChange();
        }

        FillData();
        GetNextRunTime();

        await InvokeAsync(() =>
        {
            _visible = true;
            StateHasChanged();
        });

        _form?.ResetValidation();
    }

    private async Task GetFormDataAsync()
    {
        var dto = await AlertClient.AlarmRuleService.GetAsync(_entityId) ?? new();
        _model = dto.Adapt<AlarmRuleUpsertViewModel>();
        await HandleProjectChange();
    }

    private void FillData()
    {
        if (!_model.LogMonitorItems.Any())
        {
            _model.LogMonitorItems.Add(new LogMonitorItemModel());
        }
        if (!_model.MetricMonitorItems.Any())
        {
            _model.MetricMonitorItems.Add(new MetricMonitorItemModel());
        }
        if (!_model.Items.Any())
        {
            _model.Items.Add(new AlarmRuleItemModel());
        }
    }

    private void HandleCancel()
    {
        _visible = false;
        ResetForm();
    }

    private void ResetForm()
    {
        _model = new();
        _isChange = false;
    }

    private void HandleVisibleChanged(bool val)
    {
        if (!val) HandleCancel();
    }

    private async Task SetCronExpression()
    {
        if (CronExpression.IsValidExpression(_tempCron))
        {
            _model.CheckFrequency.CronExpression = _tempCron;
            GetNextRunTime();
            _cronVisible = false;
        }
        else
        {
            await PopupService.EnqueueSnackbarAsync(T("CronExpressionInvalid"), AlertTypes.Error);
        }
    }

    private void GetNextRunTime(int showCount = 5)
    {
        if (!CronExpression.IsValidExpression(_model.CheckFrequency.CronExpression))
        {
            _nextRunTimeStr = T("CronExpressionNotHasNextRunTime");
            return;
        }

        var sb = new StringBuilder();

        var startTime = DateTimeOffset.Now;

        var cronExpression = new CronExpression(_model.CheckFrequency.CronExpression);

        var timezone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(p => p.BaseUtcOffset == JsInitVariables.TimezoneOffset);

        if (timezone != null)
            cronExpression.TimeZone = timezone;

        for (int i = 0; i < showCount; i++)
        {
            var nextExcuteTime = cronExpression.GetNextValidTimeAfter(startTime);

            if (nextExcuteTime.HasValue)
            {
                startTime = nextExcuteTime.Value;
                sb.AppendLine(string.Format("<p class='px-3 text-right'>{0}</p>", startTime.ToOffset(JsInitVariables.TimezoneOffset).ToString("yyyy-MM-dd HH:mm:ss")));
            }
        }

        if (sb.Length == 0)
        {
            _nextRunTimeStr = T("CronExpressionNotHasNextRunTime");
        }
        else
        {
            _nextRunTimeStr = sb.ToString();
        }

        StateHasChanged();
    }

    private void OpenCronModal()
    {
        _cronVisible = true;
        _tempCron = _model.CheckFrequency.CronExpression;
    }

    private void HandleLogMonitorItemsAdd(LogMonitorItemModel item)
    {
        var index = _model.LogMonitorItems.IndexOf(item) + 1;
        _model.LogMonitorItems.Insert(index, new LogMonitorItemModel());
    }

    private void HandleLogMonitorItemsRemove(LogMonitorItemModel item)
    {
        _model.LogMonitorItems.Remove(item);
    }

    private async Task HandleOk()
    {
        MasaArgumentException.ThrowIfNull(_form, "form");

        if (!_form.Validate())
        {
            return;
        }

        _isChange = true;

        _visible = false;
    }

    public async Task Submit()
    {
        if (!_isChange) return;

        var inputDto = _model.Adapt<AlarmRuleUpsertModel>();

        if (_entityId == default)
        {
            _entityId = await AlertClient.AlarmRuleService.CreateAsync(inputDto);
        }
        else
        {
            await AlertClient.AlarmRuleService.UpdateAsync(_entityId, inputDto);
        }

        ResetForm();

        if (OnOk.HasDelegate)
        {
            await OnOk.InvokeAsync(_entityId);
        }
    }

    public async Task SetIsEnabled(Guid alarmRuleId, bool isEnabled)
    {
        await AlertClient.AlarmRuleService.SetIsEnabledAsync(alarmRuleId, isEnabled);
    }

    private void HandleNextStep()
    {
        MasaArgumentException.ThrowIfNull(_form, "form");

        if (!_form.Validate())
        {
            return;
        }
        _model.Step++;
    }

    private async Task HandleProjectChange()
    {
        var projectId = _projectItems.FirstOrDefault(x => x.Identity == _model.ProjectIdentity)?.Id;
        if (projectId != null)
        {
            _appItems = await PmClient.AppService.GetListByProjectIdsAsync(new List<int> { projectId.Value }) ?? new();
        };
    }

    private async Task HandleDel()
    {
        await ConfirmAsync(T("DeletionConfirmationMessage", $"{T("AlarmRule")}\"{_model.DisplayName}\""), DeleteAsync, AlertTypes.Error);
    }

    private async Task DeleteAsync()
    {
        //Loading = true;
        await AlertClient.AlarmRuleService.DeleteAsync(_entityId);
        //Loading = false;
        OpenSuccessMessage(T("DeletedSuccessfullyMessage"));
        _visible = false;
        ResetForm();
        if (OnOk.HasDelegate)
        {
            await OnOk.InvokeAsync();
        }
    }
}
