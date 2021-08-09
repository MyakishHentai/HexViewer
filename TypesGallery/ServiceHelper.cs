using System;
using System.ServiceProcess;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public static class ServiceHelper
	{
		static ServiceHelper()
		{ }


		/// <summary>
		/// Останавливает службу, включая все зависимые
		/// </summary>
		/// <param name="service">Служба, которую нужно остановить</param>
		public static void StopService(ServiceController service, ref List<ServiceController> stoppedServices, int secondsWait = 20)
		{
			try
			{
				// Проходим вниз до самой зависимой службы
				foreach (var dependentService in service.DependentServices)
				{
					StopService(dependentService, ref stoppedServices);
				}

				// Если служба не выключена - выключаем
				if (service.Status != ServiceControllerStatus.Stopped)
				{
					service.Stop();

					// Ждём выключения
					service.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, secondsWait));

					// Заносим в список остановленных служб
					stoppedServices.Add(service);
				}
			}
			catch(Exception ex)
			{
				throw new Exception("Во время остановки служб произошла ошибка", ex);
			}
		}



		public static void StartServices(ServiceController mainService, List<ServiceController> servicesList = null, int secondsWait = 60)
		{
			try
			{
				if (mainService != null)
				{
					// На всякий случай обновим статус
					mainService.Refresh();

					// Если служба выключена - запускаем
					if (mainService.Status == ServiceControllerStatus.Stopped)
					{
						mainService.Start();

						// Ждём
						mainService.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, secondsWait));
					}
				}
			}
			catch (Exception ex)
			{
				throw new RequiredServiceException("Во время запуска основной службы произошла ошибка", ex);
			}


			// Если передали список служб, запускаем и их
			if (servicesList != null)
			{
				try
				{
					if (servicesList.Contains(mainService))
					{
						servicesList.Remove(mainService);
					}

					foreach (var service in servicesList)
					{
						// На всякий случай обновим статус
						service.Refresh();

						// Если служба выключена - запускаем
						if (service.Status == ServiceControllerStatus.Stopped)
						{
							service.Start();

							// Ждём
							service.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, secondsWait));
						}
					}
				}
				catch (Exception ex)
				{
					throw new DependentServiceException("Во время запуска зависимых служб произошла ошибка", ex);
				}
			}
		}


		/// <summary>
		/// Перезапускает службу
		/// </summary>
		/// <param name="service">Служба, которую нужно перезапустить</param>
		public static void RestartService(ServiceController service)
		{
			List<ServiceController> stoppedServices = new List<ServiceController>();

			StopService(service, ref stoppedServices);

			StartServices(service, stoppedServices);
		}
	}	
}
