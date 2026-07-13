import React, { useEffect, useRef } from 'react';
import { createPortal } from 'react-dom';
import '../styles/Form.css';

interface ModalProps {
  title: string;
  onClose: () => void;
  children: React.ReactNode;
}

/**
 * Renders a modal in a portal with guards against the same click that opened it
 * immediately closing the overlay (mousedown on button + mouseup on overlay).
 */
const Modal: React.FC<ModalProps> = ({ title, onClose, children }) => {
  const allowOverlayClose = useRef(false);

  useEffect(() => {
    const timer = window.setTimeout(() => {
      allowOverlayClose.current = true;
    }, 100);

    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };

    document.addEventListener('keydown', onKeyDown);
    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = 'hidden';

    return () => {
      window.clearTimeout(timer);
      document.removeEventListener('keydown', onKeyDown);
      document.body.style.overflow = previousOverflow;
    };
  }, [onClose]);

  const handleOverlayClick = (event: React.MouseEvent<HTMLDivElement>) => {
    if (!allowOverlayClose.current) {
      return;
    }
    if (event.target === event.currentTarget) {
      onClose();
    }
  };

  return createPortal(
    <div className="modal-overlay" onClick={handleOverlayClick} role="presentation">
      <div
        className="modal-content"
        role="dialog"
        aria-modal="true"
        aria-label={title}
        onClick={(event) => event.stopPropagation()}
      >
        <div className="modal-header">
          <h2>{title}</h2>
          <button type="button" className="close-btn" onClick={onClose} aria-label="Close">
            ✕
          </button>
        </div>
        {children}
      </div>
    </div>,
    document.body
  );
};

export default Modal;
