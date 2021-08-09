namespace Cryptosoft.TypesGallery.DataCodex
{
	public class CodexName
	{
		private readonly int m_Hash;

		public string Name { get; private set; }

		public string Product { get; private set; }

		public string Description { get; private set; }

		public CodexName(string name, string product, string description = null)
		{
			Name = name;
			Product = product;
			Description = description ?? name;

			m_Hash = MathOperations.GetHashCode(Name, Product);
		}

		public override int GetHashCode()
		{
			return m_Hash;
		}

		public override string ToString()
		{
			return Name + " by " + Product;
		}
	}
}