namespace WpfApp.Mappers
{
    using Models;
    using ViewModels;

    public class ServiceViewModelMapper
    {
        public ServiceViewModel MapFrom(ServiceModel model)
        {
            var viewModel = new ServiceViewModel(model.Name);
            UpdateViewModel(viewModel, model);
            return viewModel;
        }

        public void UpdateViewModel(ServiceViewModel viewModel, ServiceModel model)
        {
            viewModel.DisplayName = model.DisplayName;
            viewModel.Account = model.Account;
            viewModel.Status = model.Status;
            viewModel.CanPauseAndContinue = model.CanPauseAndContinue;
            viewModel.CanStop = model.CanStop;
        }
    }
}