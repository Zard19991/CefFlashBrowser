﻿using CefFlashBrowser.Models;
using CefFlashBrowser.Sol;
using CefFlashBrowser.Utils;
using SimpleMvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CefFlashBrowser.ViewModels
{
    public class SolNodeViewModel : ViewModelBase
    {
        public SolEditorWindowViewModel Editor { get; }
        public SolNodeViewModel Parent { get; }

        private object _name;
        public object Name
        {
            get => _name;
            set
            {
                if (!EqualityComparer<object>.Default.Equals(_name, value))
                {
                    _name = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(DisplayName));
                }
            }
        }

        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                if (!EqualityComparer<object>.Default.Equals(_value, value))
                {
                    UpdateValue(ref _value, value);
                    RaisePropertyChanged(nameof(TypeString));
                    UpdateChildren();
                    Parent?.OnChildrenValueChanged(this);
                    Editor?.OnNodeChanged(this);
                }
            }
        }

        private ObservableCollection<SolNodeViewModel> _children;
        public ObservableCollection<SolNodeViewModel> Children
        {
            get => _children;
            set => UpdateValue(ref _children, value);
        }

        private static Dictionary<Type, string> TypeStringDic { get; } = new Dictionary<Type, string>
        {
            [typeof(int)] = "int",
            [typeof(double)] = "double",
            [typeof(string)] = "string",
            [typeof(bool)] = "bool",
            [typeof(DateTime)] = "DateTime",
            [typeof(byte[])] = "byte[]",
            [typeof(SolArray)] = "Array",
            [typeof(SolObject)] = "Object",
            [typeof(SolUndefined)] = "undefined",
            [typeof(SolXmlDoc)] = "XmlDoc",
            [typeof(SolXml)] = "Xml"
        };

        public string TypeString
        {
            get
            {
                if (Value == null)
                {
                    return "null";
                }
                else
                {
                    var type = Value.GetType();
                    return TypeStringDic.ContainsKey(type) ? TypeStringDic[type] : string.Empty;
                }
            }
        }

        public bool IsArrayItem
        {
            get => Parent != null && Parent.Value is SolArray;
        }

        public string DisplayName
        {
            get
            {
                string name = Name == null ? "null" : Name.ToString();
                return IsArrayItem ? $"[{name}]" : name;
            }
        }

        public bool CanAddChild
        {
            get => Value is SolFileWrapper || Value is SolArray || Value is SolObject;
        }

        public bool CanRemove
        {
            get => Parent != null;
        }


        private void UpdateChildren()
        {
            var children = new ObservableCollection<SolNodeViewModel>();

            if (Value is SolFileWrapper file)
            {
                foreach (var pair in SolHelper.GetAllValues(file))
                {
                    children.Add(new SolNodeViewModel(Editor, this, pair.Key, pair.Value));
                }
            }
            else if (Value is SolArray arr)
            {
                foreach (var pair in arr.AssocPortion)
                {
                    children.Add(new SolNodeViewModel(Editor, this, pair.Key, pair.Value));
                }
                for (int i = 0; i < arr.DensePortion.Count; i++)
                {
                    children.Add(new SolNodeViewModel(Editor, this, i, arr.DensePortion[i]));
                }
            }
            else if (Value is SolObject obj)
            {
                foreach (var pair in obj.Properties)
                {
                    children.Add(new SolNodeViewModel(Editor, this, pair.Key, pair.Value));
                }
            }

            Children = children;
        }

        private void OnChildrenValueChanged(SolNodeViewModel node)
        {
            if (Value is SolArray arr)
            {
                if (node.Name is string key)
                {
                    arr.AssocPortion[key] = node.Value;
                }
                else if (node.Name is int index)
                {
                    arr.DensePortion[index] = node.Value;
                }
            }
            else if (Value is SolObject obj)
            {
                obj.Properties[node.Name.ToString()] = node.Value;
            }
        }

        public Dictionary<string, object> GetAllValues()
        {
            return Children.ToDictionary(x => x.Name.ToString(), x => x.Value);
        }

        public void Remove()
        {
            if (Parent == null)
            {
                return;
            }

            if (Parent.Value is SolArray arr)
            {
                if (Name is string key)
                {
                    arr.AssocPortion.Remove(key);
                }
                else if (Name is int index)
                {
                    arr.DensePortion.RemoveAt(index);
                }
            }
            else if (Parent.Value is SolObject obj)
            {
                obj.Properties.Remove(Name.ToString());
            }

            Parent.Children.Remove(this);
        }

        public void AddChild(object name, object value)
        {
            if (Value is SolFileWrapper)
            {
                // Do nothing
            }
            else if (Value is SolArray arr)
            {
                if (name is string key)
                {
                    arr.AssocPortion[key] = value;
                }
                else if (name is int index)
                {
                    arr.DensePortion.Insert(index, value);
                }
            }
            else if (Value is SolObject obj)
            {
                obj.Properties[name.ToString()] = value;
            }
            else
            {
                return;
            }

            Children.Add(new SolNodeViewModel(Editor, this, name, value));
        }

        public SolNodeViewModel(SolEditorWindowViewModel editor, SolNodeViewModel parent, object name, object value)
        {
            Editor = editor;
            Parent = parent;
            Name = name;
            _value = value;
            UpdateChildren();
        }

        public SolNodeViewModel(SolEditorWindowViewModel editor, SolFileWrapper file)
        {
            Editor = editor;
            Parent = null;
            Name = file.SolName;
            _value = file;
            UpdateChildren();
        }
    }
}
