using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PayValueManualSln.Application.Interfaces
{
	public interface IGenericRepositoryAsync<T> where T : class
	{
		Task<T> GetByIdAsync(int id);
		Task<IReadOnlyList<T>> GetAllAsync();
		Task<IReadOnlyList<T>> GetPagedReponseAsync(int pageNumber, int pageSize);
		Task<T> AddAsync(T entity);
		Task UpdateAsync(T entity);
		Task<T> UpdateAsync(long id);
		Task DeleteAsync(T entity);
		Task<T> DeleteAsync(long id);
	}
}
