# Quickstart: 按鈕樣式規範實作指南

**Branch**: `003-button-style` | **Date**: 2026-01-30

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022+ 或 VS Code
- 專案已可編譯執行

## Implementation Steps

### Step 1: 建立 Resources/ButtonStyles.xaml

**Path**: `NanmaoLabelPOC/Resources/ButtonStyles.xaml`

```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">

    <!-- Color Tokens (from raw_delta_button.md §4) -->
    <SolidColorBrush x:Key="BrandPrimary" Color="#1E3A5F"/>
    <SolidColorBrush x:Key="ButtonDefaultBackground" Color="#1E3A5F"/>
    <SolidColorBrush x:Key="ButtonDefaultForeground" Color="#FFFFFF"/>
    <SolidColorBrush x:Key="ButtonHoverBackground" Color="#2E4A6F"/>
    <SolidColorBrush x:Key="ButtonActiveBackground" Color="#0E2A4F"/>
    <SolidColorBrush x:Key="ButtonFocusBorder" Color="#FFFFFF"/>
    <SolidColorBrush x:Key="ButtonDisabledBackground" Color="#E0E0E0"/>
    <SolidColorBrush x:Key="ButtonDisabledForeground" Color="#A0A0A0"/>

    <!-- Secondary Button Colors -->
    <SolidColorBrush x:Key="SecondaryDefaultBackground" Color="#FFFFFF"/>
    <SolidColorBrush x:Key="SecondaryDefaultForeground" Color="#333333"/>
    <SolidColorBrush x:Key="SecondaryHoverBackground" Color="#F0F0F0"/>
    <SolidColorBrush x:Key="SecondaryBorder" Color="#E0E0E0"/>

    <!-- Feedback Colors -->
    <SolidColorBrush x:Key="FeedbackSuccess" Color="#107C10"/>
    <SolidColorBrush x:Key="FeedbackError" Color="#D13438"/>

    <!-- ActionButtonStyle -->
    <Style x:Key="ActionButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource ButtonDefaultBackground}"/>
        <Setter Property="Foreground" Value="{StaticResource ButtonDefaultForeground}"/>
        <Setter Property="BorderThickness" Value="0"/>
        <Setter Property="Padding" Value="16,8"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="border"
                            Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="4"
                            Padding="{TemplateBinding Padding}">
                        <ContentPresenter HorizontalAlignment="Center"
                                          VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <!-- Hover (Priority 6) -->
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="border" Property="Background"
                                    Value="{StaticResource ButtonHoverBackground}"/>
                        </Trigger>
                        <!-- Active (Priority 3) -->
                        <Trigger Property="IsPressed" Value="True">
                            <Setter TargetName="border" Property="Background"
                                    Value="{StaticResource ButtonActiveBackground}"/>
                        </Trigger>
                        <!-- Focus (Priority 5) -->
                        <Trigger Property="IsFocused" Value="True">
                            <Setter TargetName="border" Property="BorderBrush"
                                    Value="{StaticResource ButtonFocusBorder}"/>
                            <Setter TargetName="border" Property="BorderThickness" Value="2"/>
                        </Trigger>
                        <!-- Disabled (Priority 1 - highest, defined last) -->
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="border" Property="Background"
                                    Value="{StaticResource ButtonDisabledBackground}"/>
                            <Setter Property="Foreground"
                                    Value="{StaticResource ButtonDisabledForeground}"/>
                            <Setter Property="Cursor" Value="Arrow"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

    <!-- SecondaryButtonStyle (for ◀ ▶ navigation) -->
    <Style x:Key="SecondaryButtonStyle" TargetType="Button">
        <Setter Property="Background" Value="{StaticResource SecondaryDefaultBackground}"/>
        <Setter Property="Foreground" Value="{StaticResource SecondaryDefaultForeground}"/>
        <Setter Property="BorderBrush" Value="{StaticResource SecondaryBorder}"/>
        <Setter Property="BorderThickness" Value="1"/>
        <Setter Property="MinWidth" Value="40"/>
        <Setter Property="MinHeight" Value="40"/>
        <Setter Property="Cursor" Value="Hand"/>
        <Setter Property="Template">
            <Setter.Value>
                <ControlTemplate TargetType="Button">
                    <Border x:Name="border"
                            Background="{TemplateBinding Background}"
                            BorderBrush="{TemplateBinding BorderBrush}"
                            BorderThickness="{TemplateBinding BorderThickness}"
                            CornerRadius="4">
                        <ContentPresenter HorizontalAlignment="Center"
                                          VerticalAlignment="Center"/>
                    </Border>
                    <ControlTemplate.Triggers>
                        <Trigger Property="IsMouseOver" Value="True">
                            <Setter TargetName="border" Property="Background"
                                    Value="{StaticResource SecondaryHoverBackground}"/>
                        </Trigger>
                        <Trigger Property="IsEnabled" Value="False">
                            <Setter TargetName="border" Property="Background" Value="#F5F5F5"/>
                            <Setter Property="Foreground" Value="#C0C0C0"/>
                            <Setter Property="Cursor" Value="Arrow"/>
                        </Trigger>
                    </ControlTemplate.Triggers>
                </ControlTemplate>
            </Setter.Value>
        </Setter>
    </Style>

</ResourceDictionary>
```

### Step 2: 修改 App.xaml

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="Resources/ButtonStyles.xaml"/>
        </ResourceDictionary.MergedDictionaries>

        <!-- 既有的 Converters -->
        <converters:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
    </ResourceDictionary>
</Application.Resources>
```

### Step 3: 資料管理工具列位置分區

**DataManageView.xaml** 工具列區域：

```xml
<Grid>
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>  <!-- 建立區 -->
        <ColumnDefinition Width="*"/>     <!-- 彈性空間 -->
        <ColumnDefinition Width="Auto"/>  <!-- 危險區 -->
        <ColumnDefinition Width="*"/>     <!-- 彈性空間 -->
        <ColumnDefinition Width="Auto"/>  <!-- 確認區 -->
    </Grid.ColumnDefinitions>

    <!-- 建立區（左側） -->
    <StackPanel Grid.Column="0" Orientation="Horizontal">
        <Button Content="📥 匯入" Style="{StaticResource ActionButtonStyle}"
                Command="{Binding ImportCommand}"/>
        <Button Content="➕ 新增" Style="{StaticResource ActionButtonStyle}"
                Command="{Binding AddCommand}" Margin="8,0,0,0"/>
    </StackPanel>

    <!-- 危險區（中間，間距 32px） -->
    <Button Grid.Column="2" Content="🗑️ 刪除"
            Style="{StaticResource ActionButtonStyle}"
            Command="{Binding DeleteCommand}"
            IsEnabled="{Binding CanDelete}"
            Margin="32,0"/>

    <!-- 確認區（右側） -->
    <Button Grid.Column="4" Content="💾 儲存"
            Style="{StaticResource ActionButtonStyle}"
            Command="{Binding SaveCommand}"
            IsEnabled="{Binding CanSave}"/>
</Grid>
```

### Step 4: 脈動光暈動畫（儲存按鈕）

在 ButtonStyles.xaml 新增：

```xml
<!-- 脈動光暈樣式（儲存按鈕專用） -->
<Style x:Key="PulseActionButtonStyle" TargetType="Button"
       BasedOn="{StaticResource ActionButtonStyle}">
    <Style.Triggers>
        <DataTrigger Binding="{Binding IsDirty}" Value="True">
            <DataTrigger.EnterActions>
                <BeginStoryboard x:Name="PulseStoryboard">
                    <Storyboard RepeatBehavior="Forever">
                        <DoubleAnimation
                            Storyboard.TargetProperty="(Effect).(DropShadowEffect.Opacity)"
                            From="0.3" To="0.6" Duration="0:0:0.75"
                            AutoReverse="True">
                            <DoubleAnimation.EasingFunction>
                                <SineEase EasingMode="EaseInOut"/>
                            </DoubleAnimation.EasingFunction>
                        </DoubleAnimation>
                    </Storyboard>
                </BeginStoryboard>
            </DataTrigger.EnterActions>
            <DataTrigger.ExitActions>
                <StopStoryboard BeginStoryboardName="PulseStoryboard"/>
            </DataTrigger.ExitActions>
            <Setter Property="Effect">
                <Setter.Value>
                    <DropShadowEffect Color="White" BlurRadius="8"
                                      ShadowDepth="0" Opacity="0.3"/>
                </Setter.Value>
            </Setter>
        </DataTrigger>
    </Style.Triggers>
</Style>
```

### Step 5: ViewModel 修改

**DataManageViewModel.cs**:

```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SaveCommand))]
private bool _isDirty;

[RelayCommand]
private async Task DeleteAsync()
{
    var result = MessageBox.Show(
        "確定要刪除選取的資料嗎？此操作無法復原。",
        "確認",
        MessageBoxButton.OKCancel,
        MessageBoxImage.Warning);

    if (result == MessageBoxResult.OK)
    {
        // 執行刪除邏輯
    }
}
```

## Verification Checklist

- [ ] ActionButtonStyle 顯示深藍底白字
- [ ] Hover 時背景變亮（#2E4A6F）
- [ ] Disabled 時顯示灰色且游標為箭頭
- [ ] 工具列按鈕位置：建立區（左）| 危險區（中）| 確認區（右）
- [ ] 危險區間距 ≥ 32px
- [ ] 刪除按鈕點擊顯示確認對話框
- [ ] IsDirty=true 時儲存按鈕有脈動效果

## Troubleshooting

**Q: 樣式沒有套用？**
A: 確認 App.xaml 已正確合併 ButtonStyles.xaml

**Q: 動畫沒有播放？**
A: 確認 IsDirty 屬性有正確觸發 PropertyChanged

**Q: 按鈕狀態顯示錯誤？**
A: 檢查 Trigger 定義順序（Disabled 必須在最後）
