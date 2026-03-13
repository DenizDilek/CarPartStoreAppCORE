/**
 * Part Table Component
 * Displays parts in a table with edit and delete actions
 * @param {Array} parts - Array of parts to display
 * @param {Function} onEdit - Callback when edit button is clicked
 * @param {Function} onDelete - Callback when delete button is clicked
 */
function PartTable({ parts, onEdit, onDelete }) {
  // Format currency
  const formatCurrency = (amount) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
    }).format(amount);
  };

  // Format date
  const formatDate = (dateString) => {
    if (!dateString) return '-';
    try {
      const date = new Date(dateString);
      return date.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
      });
    } catch {
      return dateString;
    }
  };

  // Get stock status indicator
  const getStockStatus = (quantity) => {
    if (quantity === 0) return 'out-of-stock';
    if (quantity < 5) return 'low-stock';
    return 'in-stock';
  };

  return (
    <div className="part-table-container">
      <table className="part-table">
        <thead>
          <tr>
            <th>Part Number</th>
            <th>Name</th>
            <th>Category</th>
            <th>Cost Price</th>
            <th>Retail Price</th>
            <th>Stock</th>
            <th>Location</th>
            <th>Supplier</th>
            <th className="actions-column">Actions</th>
          </tr>
        </thead>
        <tbody>
          {parts.map((part) => (
            <tr key={part.id} className="part-row">
              <td className="part-number">{part.partNumber}</td>
              <td className="part-name">
                <div className="part-name-text">{part.name}</div>
                {part.description && (
                  <div className="part-description">{part.description}</div>
                )}
              </td>
              <td className="part-category">
                <span className="category-badge">{part.categoryName || 'N/A'}</span>
              </td>
              <td className="cost-price">
                {formatCurrency(part.costPrice)}
              </td>
              <td className="retail-price">
                {formatCurrency(part.retailPrice)}
              </td>
              <td className="stock">
                <span className={`stock-status ${getStockStatus(part.stockQuantity)}`}>
                  {part.stockQuantity}
                </span>
              </td>
              <td className="location">{part.location || '-'}</td>
              <td className="supplier">{part.supplier || '-'}</td>
              <td className="actions">
                <div className="action-buttons">
                  <button
                    onClick={() => onEdit(part)}
                    className="btn btn-edit"
                    title="Edit part"
                    aria-label={`Edit ${part.name}`}
                  >
                    ✏️
                  </button>
                  <button
                    onClick={() => onDelete(part.id, part.name)}
                    className="btn btn-delete"
                    title="Delete part"
                    aria-label={`Delete ${part.name}`}
                  >
                    🗑️
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
      {parts.length > 0 && (
        <div className="table-footer">
          <p className="parts-count">
            Showing {parts.length} part{parts.length !== 1 ? 's' : ''}
          </p>
        </div>
      )}
    </div>
  );
}

export default PartTable;
