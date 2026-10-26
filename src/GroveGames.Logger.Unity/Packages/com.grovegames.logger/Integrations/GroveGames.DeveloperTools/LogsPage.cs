using System;
using System.Collections.Generic;
using GroveGames.DeveloperTools.Unity;
using UnityEngine;
using UnityEngine.UIElements;

namespace GroveGames.Logger.Unity
{
    public sealed class LogsPage : IDeveloperToolsPage
    {
        private const int LevelCount = 4;
        private const string TimeFormat = "HH:mm:ss";
        private const int MinimumVisibleRows = 5;
        private const float DetailsMaxHeightPercent = 30f;

        private static readonly string[] s_levelNames = { "Debug", "Info", "Warning", "Error" };
        private static readonly Color[] s_levelColors =
        {
            new(0.62f, 0.66f, 0.72f, 1f),
            new(0.93f, 0.94f, 0.96f, 1f),
            new(1f, 0.8f, 0.3f, 1f),
            new(1f, 0.42f, 0.38f, 1f),
        };

        private readonly ILogHistory _history;
        private readonly List<LogEntry> _entries;
        private readonly List<int> _visible;
        private readonly bool[] _isLevelVisible;
        private readonly int[] _levelCounts;
        private readonly Button[] _levelButtons;
        private readonly ListView _list;
        private readonly VisualElement _details;
        private readonly TextField _detailsText;
        private int _version = -1;
        private string _searchText = string.Empty;

        public string Title => "Logs";
        public VisualElement Root { get; }

        public LogsPage(ILogHistory history)
        {
            _history = history;
            _entries = new List<LogEntry>(history.Capacity);
            _visible = new List<int>(history.Capacity);
            _isLevelVisible = new[] { true, true, true, true };
            _levelCounts = new int[LevelCount];
            _levelButtons = new Button[LevelCount];

            Root = new VisualElement();
            Root.style.flexGrow = 1f;
            Root.style.flexShrink = 1f;
            Root.style.minHeight = 0f;

            var filters = DeveloperToolsStyle.CreateRow();
            filters.style.flexWrap = Wrap.Wrap;

            for (var i = 0; i < LevelCount; i++)
            {
                var level = i;
                var button = DeveloperToolsStyle.CreateButton(s_levelNames[i], () => ToggleLevel(level));
                button.style.marginBottom = DeveloperToolsStyle.Spacing;
                _levelButtons[i] = button;
                filters.Add(button);
            }

            Root.Add(filters);

            var toolbar = DeveloperToolsStyle.CreateRow();
            var search = new TextField { style = { flexGrow = 1f, marginRight = DeveloperToolsStyle.Spacing } };
            search.textEdition.placeholder = "Search";
            search.RegisterValueChangedCallback(HandleSearchChanged);
            DeveloperToolsStyle.StyleInput(search);
            search.style.marginRight = DeveloperToolsStyle.Spacing;
            toolbar.Add(search);
            var clear = DeveloperToolsStyle.CreateButton("Clear", HandleClearClicked);
            clear.style.marginRight = 0f;
            toolbar.Add(clear);
            Root.Add(toolbar);

            _list = new ListView(_visible, DeveloperToolsStyle.RowHeight, MakeRow, BindRow)
            {
                selectionType = SelectionType.Single,
                virtualizationMethod = CollectionVirtualizationMethod.FixedHeight,
            };
            _list.style.flexGrow = 1f;
            _list.style.flexShrink = 1f;
            _list.style.flexBasis = 0f;
            _list.style.minHeight = DeveloperToolsStyle.RowHeight * MinimumVisibleRows;
            _list.style.backgroundColor = DeveloperToolsStyle.SurfaceColor;
            DeveloperToolsStyle.SetBorderRadius(_list.style, DeveloperToolsStyle.CornerRadius);
            _list.style.overflow = Overflow.Hidden;
            _list.selectionChanged += HandleSelectionChanged;
            Root.Add(_list);

            _details = new VisualElement();
            _details.style.display = DisplayStyle.None;
            _details.style.marginTop = DeveloperToolsStyle.Spacing;
            _details.style.flexShrink = 0f;
            _details.style.maxHeight = new Length(DetailsMaxHeightPercent, LengthUnit.Percent);

            _detailsText = new TextField { multiline = true, isReadOnly = true, verticalScrollerVisibility = ScrollerVisibility.Auto };
            _detailsText.style.flexGrow = 1f;
            _detailsText.style.flexShrink = 1f;
            _detailsText.style.minHeight = 0f;
            _detailsText.style.marginBottom = DeveloperToolsStyle.Spacing;
            DeveloperToolsStyle.StyleInput(_detailsText);
            var detailsInput = _detailsText.Q(className: TextField.inputUssClassName);

            if (detailsInput != null)
            {
                detailsInput.style.fontSize = DeveloperToolsStyle.FontSize;
                detailsInput.style.whiteSpace = WhiteSpace.Normal;
                detailsInput.style.paddingTop = DeveloperToolsStyle.Spacing;
                detailsInput.style.paddingBottom = DeveloperToolsStyle.Spacing;
            }

            _details.Add(_detailsText);

            var detailsButtons = DeveloperToolsStyle.CreateRow();
            detailsButtons.Add(DeveloperToolsStyle.CreateSpacer());
            detailsButtons.Add(DeveloperToolsStyle.CreateButton("Copy", HandleCopyClicked));
            var hide = DeveloperToolsStyle.CreateButton("Hide", HandleHideDetailsClicked);
            hide.style.marginRight = 0f;
            detailsButtons.Add(hide);
            _details.Add(detailsButtons);
            Root.Add(_details);
        }

        public void Refresh(bool force)
        {
            var version = _history.Version;

            if (!force && version == _version)
            {
                return;
            }

            var wasAtEnd = IsAtEnd();
            _version = version;
            _history.CopyTo(_entries);
            Array.Clear(_levelCounts, 0, LevelCount);

            for (var i = 0; i < _entries.Count; i++)
            {
                var level = (int)_entries[i].Level;

                if (level < LevelCount)
                {
                    _levelCounts[level]++;
                }
            }

            RebuildVisible();

            if (force || wasAtEnd)
            {
                ScrollToEnd();
            }
        }

        private void RebuildVisible()
        {
            _visible.Clear();

            for (var i = 0; i < _entries.Count; i++)
            {
                var entry = _entries[i];
                var level = (int)entry.Level;

                if (level < LevelCount && !_isLevelVisible[level])
                {
                    continue;
                }

                if (_searchText.Length > 0 && entry.Message.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0 && entry.Tag.IndexOf(_searchText, StringComparison.OrdinalIgnoreCase) < 0)
                {
                    continue;
                }

                _visible.Add(i);
            }

            for (var i = 0; i < LevelCount; i++)
            {
                var button = _levelButtons[i];
                var isVisible = _isLevelVisible[i];
                button.text = $"{s_levelNames[i]}  {_levelCounts[i]}";
                button.style.color = s_levelColors[i];
                button.style.opacity = isVisible ? 1f : 0.4f;
                DeveloperToolsStyle.SetBorderWidth(button.style, isVisible ? 4f : 0f);
                DeveloperToolsStyle.SetBorderColor(button.style, s_levelColors[i]);
            }

            _list.RefreshItems();
        }

        private VisualElement MakeRow()
        {
            var label = new Label();
            label.style.fontSize = DeveloperToolsStyle.FontSize;
            label.style.height = DeveloperToolsStyle.RowHeight;
            label.style.paddingLeft = DeveloperToolsStyle.LargeSpacing;
            label.style.paddingRight = DeveloperToolsStyle.LargeSpacing;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.whiteSpace = WhiteSpace.NoWrap;
            label.style.overflow = Overflow.Hidden;
            label.style.textOverflow = TextOverflow.Ellipsis;
            return label;
        }

        private void BindRow(VisualElement element, int index)
        {
            var entry = _entries[_visible[index]];
            var label = (Label)element;
            var level = (int)entry.Level;
            label.text = $"{entry.Time.ToString(TimeFormat)}  [{entry.Tag}] {FirstLine(entry.Message)}";
            label.style.color = level < LevelCount ? s_levelColors[level] : s_levelColors[(int)LogLevel.Information];
        }

        private void ToggleLevel(int level)
        {
            _isLevelVisible[level] = !_isLevelVisible[level];
            RebuildVisible();
        }

        private void HandleSearchChanged(ChangeEvent<string> changeEvent)
        {
            _searchText = changeEvent.newValue ?? string.Empty;
            RebuildVisible();
        }

        private void HandleSelectionChanged(IEnumerable<object> selection)
        {
            var index = _list.selectedIndex;

            if (index < 0 || index >= _visible.Count)
            {
                return;
            }

            var entry = _entries[_visible[index]];
            _detailsText.value = $"{entry.Time.ToString(TimeFormat)} {entry.Level} [{entry.Tag}]\n{entry.Message}";
            _details.style.display = DisplayStyle.Flex;
        }

        private void HandleCopyClicked()
        {
            GUIUtility.systemCopyBuffer = _detailsText.value;
        }

        private void HandleHideDetailsClicked()
        {
            _details.style.display = DisplayStyle.None;
            _list.ClearSelection();
        }

        private void HandleClearClicked()
        {
            _history.Clear();
            HandleHideDetailsClicked();
            Refresh(true);
        }

        private bool IsAtEnd()
        {
            var scroller = _list.Q<ScrollView>()?.verticalScroller;
            return scroller == null || scroller.value >= scroller.highValue - DeveloperToolsStyle.RowHeight;
        }

        private void ScrollToEnd()
        {
            if (_visible.Count > 0)
            {
                _list.ScrollToItem(_visible.Count - 1);
            }
        }

        private static string FirstLine(string message)
        {
            var end = message.IndexOf('\n');
            return end < 0 ? message : message.Substring(0, end);
        }
    }
}
