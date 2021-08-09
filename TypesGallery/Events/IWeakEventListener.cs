namespace Cryptosoft.TypesGallery.Events
{
	interface IWeakEventListener
	{
		void StartListen(object eventSource);

		void EndListen();
	}
}