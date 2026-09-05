/* ============================================================
   Ui — helpers de interface: formatação, sidebar, toast, modal,
   badges, loading e estados vazios.
   ============================================================ */

const Ui = {
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
    const links = [
      { key: 'dashboard', href: '/', icon: '📊', texto: 'Dashboard' },
      { key: 'produtos', href: '/pages/produtos.html', icon: '👕', texto: 'Produtos' },
      { key: 'clientes', href: '/pages/clientes.html', icon: '👤', texto: 'Clientes' },
      { key: 'vendas', href: '/pages/vendas.html', icon: '🛍️', texto: 'Vendas' },
    ];
    const navHtml = links.map(l =>
      `<a class="sidebar__link ${l.key === paginaAtiva ? 'ativo' : ''}" href="${l.href}">
         <span class="sidebar__icon">${l.icon}</span> ${l.texto}
       </a>`).join('');

    const sidebar = document.getElementById('sidebar');
    if (sidebar) {
      sidebar.innerHTML = `<div class="sidebar__brand">👗 Garimpei</div>
        <nav class="sidebar__nav">${navHtml}</nav>`;
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
