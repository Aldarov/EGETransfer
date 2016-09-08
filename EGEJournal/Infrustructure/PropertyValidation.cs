using System;

namespace EGEJournal.Infrustructure
{
    public class PropertyValidation<TBindingModel>
    {
        private Func<TBindingModel, bool> _validationCriteria;
        private string _errorMessage;
        private readonly string _propertyName;

        public PropertyValidation(string propertyName)
        {
            _propertyName = propertyName;
        }

        public PropertyValidation<TBindingModel> When(Func<TBindingModel, bool> validationCriteria)
        {
            if (_validationCriteria != null)
                throw new InvalidOperationException("Условие валидации можно указать только один раз.");
            _validationCriteria = validationCriteria;
            return this;
        }

        public PropertyValidation<TBindingModel> Show(string errorMessage)
        {
            if (_errorMessage != null)
                throw new InvalidOperationException("Для одного условия можно указать только один текст ошибки.");
            _errorMessage = errorMessage;
            return this;
        }

        public bool IsInvalid(TBindingModel presentationModel)
        {
            if (_validationCriteria == null)
                throw new InvalidOperationException("Не указаны условия валидации. (Используйте 'When(..)' метод.)");
            return _validationCriteria(presentationModel);
        }

        public string GetErrorMessage()
        {
            if (_errorMessage == null)
                throw new InvalidOperationException(
                    "Не указан текст ошибки. (Используйте 'Show(..)' метод.)");
            return _errorMessage;
        }

        public string PropertyName
        {
            get { return _propertyName; }
        }

        public bool FirstValidate = true;
    }
}
