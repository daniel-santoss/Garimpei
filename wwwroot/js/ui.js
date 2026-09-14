/* ============================================================
   Ui — helpers de interface: formatação, sidebar, toast, modal,
   badges, loading e estados vazios.
   ============================================================ */

const Ui = {
  // ----- Ícones SVG utilitários para ações -----
  icones: {
    editar: `<svg class="icone-acao" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.1" stroke-linecap="round" stroke-linejoin="round">
      <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
      <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
    </svg>`,
    excluir: `<svg class="icone-acao" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.1" stroke-linecap="round" stroke-linejoin="round">
      <polyline points="3 6 5 6 21 6"></polyline>
      <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
      <line x1="10" y1="11" x2="10" y2="17"></line>
      <line x1="14" y1="11" x2="14" y2="17"></line>
    </svg>`,
    desativar: `<svg class="icone-acao" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.1" stroke-linecap="round" stroke-linejoin="round">
      <circle cx="12" cy="12" r="10"></circle>
      <line x1="4.93" y1="4.93" x2="19.07" y2="19.07"></line>
    </svg>`,
    ativar: `<svg class="icone-acao" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round">
      <circle cx="12" cy="12" r="10"></circle>
      <polyline points="9 12 11 14 15 10"></polyline>
    </svg>`,
    detalhes: `<svg class="icone-acao" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.1" stroke-linecap="round" stroke-linejoin="round">
      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
      <circle cx="12" cy="12" r="3"></circle>
    </svg>`,
    cancelar: `<svg class="icone-acao" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.1" stroke-linecap="round" stroke-linejoin="round">
      <circle cx="12" cy="12" r="10"></circle>
      <line x1="15" y1="9" x2="9" y2="15"></line>
      <line x1="9" y1="9" x2="15" y2="15"></line>
    </svg>`
  },

  // ----- Formatação -----
  moeda(v) {
    const n = Number(v || 0);
    return n.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
  },
  data(iso) {
    if (!iso) return '';
    const d = new Date(iso);
    if (isNaN(d)) return '';
    return d.toLocaleDateString('pt-BR');
  },
  escape(s) {
    if (s === null || s === undefined) return '';
    return String(s)
      .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;').replace(/'/g, '&#39;');
  },

  // ----- Badge de status -----
  badge(texto) {
    const mapa = {
      'Disponível': 'disponivel', 'Vendido': 'vendido', 'Inativo': 'inativo',
      'Concluída': 'concluida', 'Cancelada': 'cancelada'
    };
    const cls = mapa[texto] || 'vendido';
    return `<span class="badge badge--${cls}">${this.escape(texto)}</span>`;
  },

  // ----- Toast -----
  toast(mensagem, tipo = 'sucesso') {
    let area = document.querySelector('.toast-area');
    if (!area) {
      area = document.createElement('div');
      area.className = 'toast-area';
      document.body.appendChild(area);
    }
    const t = document.createElement('div');
    t.className = `toast toast--${tipo}`;
    t.textContent = mensagem;
    area.appendChild(t);
    setTimeout(() => { t.style.opacity = '0'; setTimeout(() => t.remove(), 250); }, 3000);
  },
  sucesso(msg) { this.toast(msg, 'sucesso'); },
  erro(msg) { this.toast(msg, 'erro'); },

  // ----- Modal genérico -----
  // Retorna o elemento overlay; feche com Ui.fecharModal(overlay).
  abrirModal(titulo, htmlCorpo, htmlRodape) {
    const overlay = document.createElement('div');
    overlay.className = 'modal-overlay';
    overlay.innerHTML = `
      <div class="modal">
        <div class="modal__head">
          <h3>${this.escape(titulo)}</h3>
          <button class="modal__fechar" aria-label="Fechar">&times;</button>
        </div>
        <div class="modal__body">${htmlCorpo}</div>
        ${htmlRodape ? `<div class="modal__foot">${htmlRodape}</div>` : ''}
      </div>`;
    document.body.appendChild(overlay);
    const fechar = () => this.fecharModal(overlay);
    overlay.querySelector('.modal__fechar').addEventListener('click', fechar);
    overlay.addEventListener('click', (e) => { if (e.target === overlay) fechar(); });
    return overlay;
  },
  fecharModal(overlay) { if (overlay && overlay.parentNode) overlay.remove(); },

  // ----- Confirmação (Promise<boolean>) -----
  confirmar(mensagem, textoBotao = 'Confirmar') {
    return new Promise((resolve) => {
      const rodape = `
        <button class="btn btn--secundario" data-acao="cancelar">Cancelar</button>
        <button class="btn btn--perigo" data-acao="ok">${this.escape(textoBotao)}</button>`;
      const overlay = this.abrirModal('Confirmação', `<p>${this.escape(mensagem)}</p>`, rodape);
      overlay.querySelector('[data-acao="cancelar"]').addEventListener('click', () => { this.fecharModal(overlay); resolve(false); });
      overlay.querySelector('[data-acao="ok"]').addEventListener('click', () => { this.fecharModal(overlay); resolve(true); });
    });
  },

  // ----- Loading em container -----
  loading(el) { if (el) el.innerHTML = '<div class="spinner"></div>'; },
  vazio(el, texto, emoji = '📭') {
    if (el) el.innerHTML = `<div class="estado-vazio"><span class="emoji">${emoji}</span>${this.escape(texto)}</div>`;
  },

  // ----- Sidebar / layout -----
  montarLayout(paginaAtiva) {
    const icons = {
      dashboard: `<svg class="sidebar__icon-svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
        <rect x="3" y="3" width="7" height="9" rx="1.5"></rect>
        <rect x="14" y="3" width="7" height="5" rx="1.5"></rect>
        <rect x="14" y="12" width="7" height="9" rx="1.5"></rect>
        <rect x="3" y="16" width="7" height="5" rx="1.5"></rect>
      </svg>`,
      produtos: `<svg class="sidebar__icon-svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
        <path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"></path>
        <line x1="7" y1="7" x2="7.01" y2="7"></line>
      </svg>`,
      clientes: `<svg class="sidebar__icon-svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
        <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"></path>
        <circle cx="9" cy="7" r="4"></circle>
        <path d="M22 21v-2a4 4 0 0 0-3-3.87"></path>
        <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
      </svg>`,
      vendas: `<svg class="sidebar__icon-svg" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
        <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z"></path>
        <line x1="3" y1="6" x2="21" y2="6"></line>
        <path d="M16 10a4 4 0 0 1-8 0"></path>
      </svg>`,
      hanger: `<svg class="sidebar__logo-svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round">
        <path d="M12 3a2.5 2.5 0 0 1 2.5 2.5c0 1.2-.8 2.2-2 2.4V9"></path>
        <path d="M2.5 17L11.4 9.8a1 1 0 0 1 1.2 0L21.5 17a1 1 0 0 1-.6 1.8H3.1a1 1 0 0 1-.6-1.8z"></path>
        <line x1="3.5" y1="18.8" x2="20.5" y2="18.8"></line>
      </svg>`
    };

    const grupos = [
      {
        titulo: 'Visão Geral',
        links: [
          { key: 'dashboard', href: '/', icon: icons.dashboard, texto: 'Dashboard' }
        ]
      },
      {
        titulo: 'Gestão do Brechó',
        links: [
          { key: 'produtos', href: '/pages/produtos.html', icon: icons.produtos, texto: 'Produtos' },
          { key: 'clientes', href: '/pages/clientes.html', icon: icons.clientes, texto: 'Clientes' },
          { key: 'vendas', href: '/pages/vendas.html', icon: icons.vendas, texto: 'Vendas' }
        ]
      }
    ];

    const navHtml = grupos.map(g => `
      <div class="sidebar__group">
        <div class="sidebar__section-title">${g.titulo}</div>
        <div class="sidebar__nav-list">
          ${g.links.map(l => `
            <a class="sidebar__link ${l.key === paginaAtiva ? 'ativo' : ''}" href="${l.href}">
              <span class="sidebar__icon">${l.icon}</span>
              <span class="sidebar__text">${l.texto}</span>
            </a>
          `).join('')}
        </div>
      </div>
    `).join('');

    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
      sidebar.innerHTML = `
        <a class="sidebar__brand-link" href="/" title="Garimpei — Gestão de Brechó">
          <div class="sidebar__logo-mark">
            ${icons.hanger}
          </div>
          <div class="sidebar__brand-text">
            <span class="sidebar__brand-name">Garimpei</span>
            <span class="sidebar__brand-tag">BRECHÓ & VINTAGE</span>
          </div>
        </a>
        <nav class="sidebar__nav">
          ${navHtml}
        </nav>
        <div class="sidebar__footer">
          <div class="sidebar__store">
            <div class="sidebar__store-avatar">BG</div>
            <div class="sidebar__store-info">
              <span class="sidebar__store-name">Garimpei Matriz</span>
              <span class="sidebar__store-status">
                <span class="sidebar__status-dot"></span>
                <span>Operação Ativa</span>
              </span>
            </div>
          </div>
        </div>
      `;
    }
    // Toggle mobile
    const burger = document.getElementById('burger');
    const backdrop = document.getElementById('backdrop');
    if (burger && sidebar && backdrop) {
      const abrir = () => { sidebar.classList.add('aberta'); backdrop.classList.add('ativo'); };
      const fechar = () => { sidebar.classList.remove('aberta'); backdrop.classList.remove('ativo'); };
      burger.addEventListener('click', abrir);
      backdrop.addEventListener('click', fechar);
    }
  },
};
