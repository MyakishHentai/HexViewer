using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace Cryptosoft.TypesGallery
{
	public class BlocksStreamException : ApplicationException
	{
		public BlocksStreamException(string message)
			: base(message) { }

		public BlocksStreamException(string message, Exception ex)
			: base(message, ex) { }
	}

	public class BlocksStreamCorruptedException : ApplicationException
	{
		public BlocksStreamCorruptedException(string message)
			: base(message) { }

		public BlocksStreamCorruptedException(string message, Exception ex)
			: base(message, ex) { }
	}



	/// <summary>
	/// Поток принимает данные, снабжает заголовком и пишет в другой поток.	В конце пишется соответсвующий признак.
	/// </summary>
	public class BlocksWriterStream : Stream
	{
		public const uint HeaderMarker = 0xE2E4E8E6;
		public const uint DataMarker = 0xE1E4E8E8;


		public interface IHeaderBuilder
		{
			int HeaderSize
			{ get; }

			/// <returns>Размер чистых данных</returns>
			int ParseHeader(Byte[] data);
			Byte[] BuildHeader(Byte[] data, int offset, int length);
			Byte[] BuildEnd();
            Byte[] BuildErrorHeader();
		}

		public class SimpleHeaderBuilder : IHeaderBuilder
		{
			public int HeaderSize
			{
				get { return sizeof(int); }
			}

			public int ParseHeader(byte[] data)
			{
				if (data == null || data.Length != HeaderSize)
					throw new ArgumentException("data");

				return BitConverter.ToInt32(data, 0);
			}

			public byte[] BuildHeader(byte[] data, int offset, int length)
			{
				return BitConverter.GetBytes(length);
			}

			public byte[] BuildEnd()
			{
				return BitConverter.GetBytes(0);
			}

            public byte[] BuildErrorHeader()
            {
                return BitConverter.GetBytes(int.MaxValue);
            }
		}

		IHeaderBuilder m_HeaderBuilder;
		Stream m_Destination;
		int m_Position = 0;

		public BlocksWriterStream(Stream destination)
			: this(new SimpleHeaderBuilder(), destination)
		{ }

		public BlocksWriterStream(IHeaderBuilder builder, Stream destination)
		{
			m_HeaderBuilder = builder;
			m_Destination = destination;
		}

	#region typical Stream implimentation

		public override bool CanRead
		{ get { return false; } }

		public override bool CanSeek
		{ get { return false; } }

		public override bool CanWrite
		{ get { return true; } }

		public override void Flush()
		{ }

		public override long Length
		{
			get { return m_Position; }
		}

		public override long Position
		{
			get { return m_Position; }
			set { throw new NotImplementedException(); }
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

	#endregion

		public override void Write(byte[] buffer, int offset, int count)
		{
			var Header = m_HeaderBuilder.BuildHeader(buffer, offset, count);
			m_Destination.Write(BitConverter.GetBytes(HeaderMarker), 0, BitConverter.GetBytes(HeaderMarker).Length);
			m_Destination.Write(Header, 0, Header.Length);

			m_Destination.Write(BitConverter.GetBytes(DataMarker), 0, BitConverter.GetBytes(DataMarker).Length);
			m_Destination.Write(buffer, offset, count);
		}

		public override void Close()
		{
			base.Close();

			var End = m_HeaderBuilder.BuildEnd();
			m_Destination.Write(BitConverter.GetBytes(HeaderMarker), 0, BitConverter.GetBytes(HeaderMarker).Length);
			m_Destination.Write(End, 0, End.Length);
		}

        public void Close(Exception ex)
        {
            base.Close();

            var End = m_HeaderBuilder.BuildErrorHeader();
			m_Destination.Write(BitConverter.GetBytes(HeaderMarker), 0, BitConverter.GetBytes(HeaderMarker).Length);
            m_Destination.Write(End, 0, End.Length);
                       
            var BinFormatter = new BinaryFormatter();
            BinFormatter.Serialize(m_Destination, ex);         
        }
        		
		public new Task WriteAsync(byte[] buffer, int offset, int count)
		{
			return Task.Run(() =>
			{
				var Header = m_HeaderBuilder.BuildHeader(buffer, offset, count);
				m_Destination.WriteAsync(BitConverter.GetBytes(HeaderMarker), 0, BitConverter.GetBytes(HeaderMarker).Length).Wait();
				m_Destination.Write(Header, 0, Header.Length);

				m_Destination.WriteAsync(BitConverter.GetBytes(DataMarker), 0, BitConverter.GetBytes(DataMarker).Length).Wait();
				m_Destination.Write(buffer, offset, count);
			});
		}
		
		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			return Task.Run(() =>
			{
				var Header = m_HeaderBuilder.BuildHeader(buffer, offset, count);
				m_Destination.WriteAsync(BitConverter.GetBytes(HeaderMarker), 0, BitConverter.GetBytes(HeaderMarker).Length).Wait();
				m_Destination.WriteAsync(Header, 0, Header.Length, cancellationToken).Wait();

				m_Destination.WriteAsync(BitConverter.GetBytes(DataMarker), 0, BitConverter.GetBytes(DataMarker).Length).Wait();
				m_Destination.WriteAsync(buffer, offset, count, cancellationToken).Wait();
			});
		}
	}

	/// <summary>
	/// Читает данные, сформированные BlocksWriterStream, понимает заголовки, и пишет чистые данные в другой поток.
	/// Понимает, когда данные закончились.
	/// </summary>
	public class BlocksReader
	{
		BlocksWriterStream.IHeaderBuilder m_Builder;
		Stream m_Source;
		Stream m_Destination;

		public BlocksReader(Stream source, Stream destination)
			: this(new BlocksWriterStream.SimpleHeaderBuilder(), source, destination)
		{ }

		public BlocksReader(BlocksWriterStream.IHeaderBuilder builder, Stream source, Stream destination)
		{
			m_Builder = builder;
			m_Source = source;
			m_Destination = destination;
		}

		public void Start()
		{
			try
			{
				StartAsync().Wait();
			}
			catch (AggregateException aex)
			{
				if (aex.InnerException is BlocksStreamException)
				{
					throw new ApplicationException(String.Format("{0}:\n{1}", aex.InnerException.Message, aex.InnerException.InnerException.Message));
				}
				else
				{
					throw aex.InnerException;
				}
			}
		}

		public Task StartAsync()
		{
			return StartAsync(CancellationToken.None);
		}

		public Task StartAsync(CancellationToken cToken)
		{
			

			return Task.Run(() =>
			{
				while (true)
				{
					// Читаем маркер заголовка
					var RawMarker = new Byte[sizeof(uint)];
					m_Source.ReadAsync(RawMarker, 0, RawMarker.Length, cToken).Wait();

					uint Marker = BitConverter.ToUInt32(RawMarker, 0);

					// Если маркер не соответствует константемшыг заголовка - - данные испорчены
					if (Marker != BlocksWriterStream.HeaderMarker)
					{
						throw new BlocksStreamCorruptedException("Целостность полученных данных нарушена");
					}

					// Читаем заголовок
					var Header = new Byte[m_Builder.HeaderSize];
					m_Source.ReadAsync(Header, 0, Header.Length, cToken).Wait();

					var DataLength = m_Builder.ParseHeader(Header);
					if (DataLength == 0)
					{
						// Конец
						break;
					}
                    else if (DataLength == int.MaxValue)
                    {
                        var BinFormatter = new BinaryFormatter();
                        var Ex = (Exception)BinFormatter.Deserialize(m_Source);
						throw new BlocksStreamException("На удалённом узле произошла ошибка", Ex);
                    }
                    else
                    {
						// Читаем маркер данных
						m_Source.ReadAsync(RawMarker, 0, RawMarker.Length, cToken).Wait();

						Marker = BitConverter.ToUInt32(RawMarker, 0);

						// Если маркер не соответствует константе данных - данные испорчены
						if (Marker != BlocksWriterStream.DataMarker)
						{
							throw new BlocksStreamCorruptedException("Целостность полученных данных нарушена");
						}

						var Data = new Byte[DataLength];

						// Т.к. работаем с сеткой, мы не можем гарантировать, что Read прочитает столько, сколько мы запросили
						// Поэтому отталкиваемся от возвращаемого зачения
						for (int i = 0; i < DataLength;)
						{
							var ReadTask = m_Source.ReadAsync(Data, 0, Data.Length - i, cToken);
							ReadTask.Wait();

							i += ReadTask.Result;
						}

						m_Destination.WriteAsync(Data, 0, DataLength, cToken).Wait();

                    }
				}
			});
		}
	}
}
