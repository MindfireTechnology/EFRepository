using MindfireClientDashboard.Data;
using Nelibur.ObjectMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests.Serialized;

namespace UnitTests.Data
{
	public static class DataExtensions
	{
		public static IQueryable<User> ByUserId(this IQueryable<User> query, int? userId)
		{
			if (userId != null)
				return query.Where(n => n.ID == userId.Value);

			return query;
		}

		public static IQueryable<User> ByName(this IQueryable<User> query, string firstName, string lastName)
		{
			if (!string.IsNullOrWhiteSpace(firstName))
				query = query.Where(n => n.FirstName == firstName);

			if (!string.IsNullOrWhiteSpace(lastName))
				query = query.Where(n => n.LastName == lastName);

			return query;
		}

		public static UserDTO ToDto(this User value)
		{
			return TinyMapper.Map<UserDTO>(value);
		}

		public static IEnumerable<UserDTO> ToDto(this IEnumerable<User> values)
		{
			return values.Select(n => n.ToDto());
		}
	}
}
