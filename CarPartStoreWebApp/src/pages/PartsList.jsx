/**
 * Parts List Page
 * Main page for viewing and managing car parts inventory
 */

import { useState } from 'react';
import { useNavigate, useParams, Link } from 'react-router-dom';
import {
  useParts,
  useCategories,
  useDeletePart,
  queryKeys,
} from '../hooks/useParts';
import CategoryFilter from '../components/CategoryFilter';
import SearchBar from '../components/SearchBar';
import PartTable from '../components/PartTable';
import PartForm from '../components/PartForm';

/**
 * Parts List Component
 */
function PartsList() {
  const navigate = useNavigate();
  const { categoryId } = useParams();

  // State for filters
  const [searchTerm, setSearchTerm] = useState('');
  const [selectedCategoryId, setSelectedCategoryId] = useState(
    categoryId ? parseInt(categoryId) : undefined
  );

  // Fetch data with React Query
  const {
    data: parts,
    isLoading: partsLoading,
    error: partsError,
    refetch: refetchParts,
  } = useParts({
    search: searchTerm,
    categoryId: selectedCategoryId,
  });

  const {
    data: categories,
    isLoading: categoriesLoading,
    error: categoriesError,
  } = useCategories();

  // Mutation for deleting parts
  const deletePartMutation = useDeletePart();

  // State for part form modal
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingPart, setEditingPart] = useState(null);

  // Handle part deletion
  const handleDeletePart = async function(partId, partName) {
    if (window.confirm('Are you sure you want to delete "' + partName + '"?')) {
      try {
        await deletePartMutation.mutateAsync(partId);
      } catch (error) {
        console.error('Failed to delete part:', error);
        alert('Failed to delete part. Please try again.');
      }
    }
  };

  // Handle opening form for new part
  const handleAddPart = function() {
    setEditingPart(null);
    setIsModalOpen(true);
  };

  // Handle opening form for editing part
  const handleEditPart = function(part) {
    setEditingPart(part);
    setIsModalOpen(true);
  };

  // Handle closing modal
  const handleCloseModal = function() {
    setIsModalOpen(false);
    setEditingPart(null);
  };

  // Handle form submission
  const handleFormSubmit = function() {
    setIsModalOpen(false);
    setEditingPart(null);
    refetchParts();
  };

  // Loading state
  if (partsLoading || categoriesLoading) {
    return (
      <div className="loading-container">
        <div className="spinner"></div>
        <p>Loading inventory...</p>
      </div>
    );
  }

  // Error state
  if (partsError) {
    return (
      <div className="error-container">
        <h2>Error</h2>
        <p>Failed to load parts: {partsError.message}</p>
        <button onClick={() => refetchParts()} className="btn btn-primary">
          Retry
        </button>
      </div>
    );
  }

  // Main content
  return (
    <div className="parts-list-page">
      <div className="page-header">
        <h1 className="page-title">
          {selectedCategoryId ? 'Category Parts' : 'All Parts'}
        </h1>
        <button onClick={handleAddPart} className="btn btn-primary">
          + Add Part
        </button>
      </div>

      <div className="filters-container">
        <div className="filter-section">
          <SearchBar
            value={searchTerm}
            onChange={setSearchTerm}
            placeholder="Search parts by name, number, or supplier..."
          />
        </div>
        <div className="filter-section">
          <CategoryFilter
            categories={categories || []}
            selectedCategoryId={selectedCategoryId}
            onCategoryChange={function(id) {
              setSelectedCategoryId(id);
              if (id) {
                navigate('/category/' + id);
              } else {
                navigate('/');
              }
            }}
          />
        </div>
      </div>

      {parts && parts.length > 0 ? (
        <PartTable
          parts={parts}
          onEdit={handleEditPart}
          onDelete={handleDeletePart}
        />
      ) : parts && parts.length === 0 ? (
        <div className="empty-state">
          <h3>No Parts Found</h3>
          <p>
            {searchTerm || selectedCategoryId
              ? 'No parts match your search criteria.'
              : 'No parts in inventory yet. Add your first part to get started.'}
          </p>
          <button onClick={handleAddPart} className="btn btn-primary">
            Add Your First Part
          </button>
        </div>
      ) : null}

      {isModalOpen && (
        <div className="modal-overlay" onClick={handleCloseModal}>
          <div className="modal-content" onClick={function(e) {
            e.stopPropagation();
          }}>
            <div className="modal-header">
              <h2>
                {editingPart ? 'Edit Part' : 'Add New Part'}
              </h2>
              <button onClick={handleCloseModal} className="close-btn">
                ✕
              </button>
            </div>
            <div className="modal-body">
              <PartForm
                part={editingPart}
                categories={categories || []}
                onSubmit={handleFormSubmit}
                onCancel={handleCloseModal}
              />
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default PartsList;
