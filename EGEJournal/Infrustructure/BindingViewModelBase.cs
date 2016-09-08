using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections;
using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace EGEJournal.Infrustructure
{
    public abstract class BindingViewModelBase<TBindingModel> : ViewModelBase, IDataErrorInfo
        where TBindingModel : BindingViewModelBase<TBindingModel>
    {
        private readonly List<PropertyValidation<TBindingModel>> ListValidations = new List<PropertyValidation<TBindingModel>>();

        protected BindingViewModelBase()
        {
        }

        private static string GetPropertyName(Expression<Func<object>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");
            MemberExpression memberExpression;
            if (expression.Body is UnaryExpression)
                memberExpression = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            else
                memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException("The expression is not a member access expression", "expression");
            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
                throw new ArgumentException("The member access expression does not access a property", "expression");
            var getMethod = property.GetGetMethod(true);
            if (getMethod.IsStatic)
                throw new ArgumentException("The referenced property is a static property", "expression");
            return memberExpression.Member.Name;
        }

        protected PropertyValidation<TBindingModel> AddValidationFor(Expression<Func<object>> expression)
        {
            return AddValidationFor(GetPropertyName(expression));
        }

        private PropertyValidation<TBindingModel> AddValidationFor(string propertyName)
        {
            var validation = new PropertyValidation<TBindingModel>(propertyName);
            ListValidations.Add(validation);
            return validation;
        }

        public void ClearValidationRoles()
        {
            ListValidations.Clear();
        }

        private List<string> ValidationPropertyNames
        {
            get
            {
                return ListValidations.Select(q => q.PropertyName).Distinct().ToList();
            }
        }

        protected void AddError(string property, string error)
        {
            DelError(property);
            ListErrors.Add(new KeyValue() { Key = property, Value = error });
        }

        protected void DelError(string property)
        {
            KeyValue prop_error = ListErrors.Where(x => x.Key == property).FirstOrDefault();
            if (prop_error != null)
            {
                ListErrors.Remove(prop_error);
            }
        }

        private ObservableCollection<KeyValue> _ListErrors;
        public ObservableCollection<KeyValue> ListErrors
        {
            get
            {
                if (_ListErrors == null)
                {
                    ListErrors = new ObservableCollection<KeyValue>();
                    ListErrors.CollectionChanged += (s, e) =>
                    {
                        RaisePropertyChanged(() => IsModelValid);
                    };
                }
                return _ListErrors;
            }
            set
            {
                if (value != _ListErrors)
                {
                    _ListErrors = value;
                    RaisePropertyChanged(() => ListErrors);
                }
            }
        }

        public Boolean IsModelValid
        {
            get
            {
                if (ListErrors != null && ListErrors.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        protected Boolean ModelValidate()
        {
            Boolean isValid = true;
            ListErrors.Clear();
            foreach (string prop_name in ValidationPropertyNames)
            {
                if (this[prop_name] != null)
                {
                    isValid = false;
                }
            }
            return isValid;
        }

        #region IDataErrorInfo
        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get 
            {
                string result = null;

                IEnumerable<PropertyValidation<TBindingModel>> validations = ListValidations.Where(x => x.PropertyName == columnName);
                foreach (PropertyValidation<TBindingModel> item in validations)
                {
                    if (item.FirstValidate)
                    {
                        item.FirstValidate = false;
                        continue;
                    }
                    if (item.IsInvalid((TBindingModel)this))
                    {
                        result = item.GetErrorMessage();
                        break;
                    }
                }

                if (result == null)
                {
                    DelError(columnName);
                }
                else
                {
                    AddError(columnName, result);
                }
                return result;
            }
        }
        #endregion
    }

    public class KeyValue
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
