using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFRepository.Generator.IntegrationTests
{
	public record Post
	{
		public int Id { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }

		public Post()
		{
			Title = string.Empty;
			Content = string.Empty;
		}

	}
}
