namespace Cryptosoft.TypesGallery.Commands
{
	public static class DataCommands
	{
		public static CommandDescriptor Load { get; private set; }
		public static CommandDescriptor Save { get; private set; }
		public static CommandDescriptor Update { get; private set; }

		static DataCommands()
		{
			Load = new CommandDescriptor("Load", typeof(DataCommands));
			Save = new CommandDescriptor("Save", typeof(DataCommands));
			Update = new CommandDescriptor("Update", typeof(DataCommands));
		}
	}
}
