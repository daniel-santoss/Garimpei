Ui.montarLayout('produtos');

const elLista = document.getElementById('lista');
const fBusca = document.getElementById('f-busca');
const fCategoria = document.getElementById('f-categoria');
const fStatus = document.getElementById('f-status');
let debounce;

async function carregarCategoriasFiltro() {
  try {
    const cats = await Api.get('/api/categorias');
    fCategoria.insertAdjacentHTML('beforeend',
      cats.map(c => `<option value="${c.id}">${Ui.escape(c.nome)}</option>`).join(''));
  } catch { /* filtro segue só com "Todas" */ }
}

async function carregarProdutos() {
  Ui.loading(elLista);
  const params = new URLSearchParams();
  if (fBusca.value.trim()) params.append('busca', fBusca.value.trim());
  if (fCategoria.value) params.append('categoriaId', fCategoria.value);
  if (fStatus.value) params.append('status', fStatus.value);

  try {
    const produtos = await Api.get('/api/produtos?' + params.toString());
    renderTabela(produtos);
  } catch (e) {
    Ui.erro(e.message);
    Ui.vazio(elLista, 'Não foi possível carregar os produtos.', '⚠️');
  }
}

function renderTabela(produtos) {
  if (!produtos || produtos.length === 0) {
    Ui.vazio(elLista, 'Nenhum produto encontrado.', '👕');
    return;
  }
  elLista.innerHTML = `
    <div class="tabela-wrap">
      <table class="tabela">
        <thead>
          <tr>
            <th>Nome</th><th>Categoria</th><th>Tam.</th><th>Cor</th>
            <th class="num">Preço</th><th>Estado</th><th>Status</th><th class="acoes">Ações</th>
          </tr>
        </thead>
        <tbody>
          ${produtos.map(p => linha(p)).join('')}
        </tbody>
      </table>
    </div>`;

  elLista.querySelectorAll('[data-excluir]').forEach(b =>
    b.addEventListener('click', () => excluir(b.dataset.excluir, b.dataset.nome)));
  elLista.querySelectorAll('[data-toggle]').forEach(b =>
    b.addEventListener('click', () => alternarStatus(b.dataset.toggle, Number(b.dataset.status))));
}

function linha(p) {
  const vendido = p.status === 2;
  // Alterna entre Disponível(1) e Inativo(3). Bloqueado se Vendido.
  const proximo = p.status === 1 ? 3 : 1;
  const tituloToggle = vendido ? 'Produto vendido' : (p.status === 1 ? 'Tornar inativo' : 'Tornar disponível');
  const iconeToggle = p.status === 1 ? Ui.icones.desativar : Ui.icones.ativar;
  const classeToggle = p.status === 1 ? 'btn-icon--inativar' : 'btn-icon--ativar';
  return `
    <tr>
      <td>${Ui.escape(p.nome)}</td>
      <td>${Ui.escape(p.categoriaNome)}</td>
      <td>${Ui.escape(p.tamanho || '—')}</td>
      <td>${Ui.escape(p.cor || '—')}</td>
      <td class="num">${Ui.moeda(p.preco)}</td>
      <td>${Ui.escape(p.estadoTexto)}</td>
      <td>${Ui.badge(p.statusTexto)}</td>
      <td class="acoes">
        <button class="btn-icon ${classeToggle}" title="${tituloToggle}" data-toggle="${p.id}" data-status="${proximo}" ${vendido ? 'disabled' : ''}>${iconeToggle}</button>
        <a class="btn-icon btn-icon--editar" title="Editar" href="/pages/produto-form.html?id=${p.id}">${Ui.icones.editar}</a>
        <button class="btn-icon btn-icon--excluir" title="Excluir" data-excluir="${p.id}" data-nome="${Ui.escape(p.nome)}">${Ui.icones.excluir}</button>
      </td>
    </tr>`;
}

async function excluir(id, nome) {
  const ok = await Ui.confirmar(`Excluir o produto "${nome}"?`, 'Excluir');
  if (!ok) return;
  try {
    await Api.del('/api/produtos/' + id);
    Ui.sucesso('Produto excluído.');
    carregarProdutos();
  } catch (e) { Ui.erro(e.message); }
}

async function alternarStatus(id, novoStatus) {
  try {
    await Api.patch(`/api/produtos/${id}/status`, { status: novoStatus });
    Ui.sucesso('Disponibilidade atualizada.');
    carregarProdutos();
  } catch (e) { Ui.erro(e.message); }
}

/* ---------- Modal: gerenciar categorias ---------- */
document.getElementById('btn-categorias').addEventListener('click', abrirCategorias);

async function abrirCategorias() {
  const corpo = `
    <div class="campo" style="flex-direction:row; gap:8px; align-items:flex-end; margin-bottom:16px;">
      <div style="flex:1;">
        <label for="nova-cat">Nova categoria</label>
        <input type="text" id="nova-cat" placeholder="Nome da categoria" />
      </div>
      <button class="btn btn--primario" id="add-cat">Adicionar</button>
    </div>
    <div id="cat-lista"><div class="spinner"></div></div>`;
  const overlay = Ui.abrirModal('Gerenciar categorias', corpo);

  const listaEl = overlay.querySelector('#cat-lista');
  const inputEl = overlay.querySelector('#nova-cat');

  async function recarregar() {
    Ui.loading(listaEl);
    try {
      const cats = await Api.get('/api/categorias');
      if (cats.length === 0) { Ui.vazio(listaEl, 'Nenhuma categoria.', '🏷️'); return; }
      listaEl.innerHTML = `<ul class="lista-simples">
        ${cats.map(c => `
          <li>
            <span>${Ui.escape(c.nome)} <span class="muted">· ${c.qtdProdutos} produto(s)</span></span>
            <button class="btn-icon btn-icon--excluir" title="Excluir" data-del="${c.id}" data-nome="${Ui.escape(c.nome)}">${Ui.icones.excluir}</button>
          </li>`).join('')}
      </ul>`;
      listaEl.querySelectorAll('[data-del]').forEach(b =>
        b.addEventListener('click', () => excluirCat(b.dataset.del, b.dataset.nome, recarregar)));
    } catch (e) { Ui.erro(e.message); }
  }

  overlay.querySelector('#add-cat').addEventListener('click', async () => {
    const nome = inputEl.value.trim();
    if (!nome) { Ui.erro('Informe o nome da categoria.'); return; }
    try {
      await Api.post('/api/categorias', { nome });
      Ui.sucesso('Categoria criada.');
      inputEl.value = '';
      recarregar();
      atualizarFiltroCategorias();
    } catch (e) { Ui.erro(e.message); }
  });
  inputEl.addEventListener('keydown', (e) => { if (e.key === 'Enter') overlay.querySelector('#add-cat').click(); });

  recarregar();
}

async function excluirCat(id, nome, recarregar) {
  const ok = await Ui.confirmar(`Excluir a categoria "${nome}"?`, 'Excluir');
  if (!ok) return;
  try {
    await Api.del('/api/categorias/' + id);
    Ui.sucesso('Categoria excluída.');
    recarregar();
    atualizarFiltroCategorias();
  } catch (e) { Ui.erro(e.message); }
}

async function atualizarFiltroCategorias() {
  const atual = fCategoria.value;
  fCategoria.innerHTML = '<option value="">Todas</option>';
  await carregarCategoriasFiltro();
  fCategoria.value = atual;
}

/* ---------- Filtros ---------- */
fBusca.addEventListener('input', () => { clearTimeout(debounce); debounce = setTimeout(carregarProdutos, 300); });
fCategoria.addEventListener('change', carregarProdutos);
fStatus.addEventListener('change', carregarProdutos);

carregarCategoriasFiltro();
carregarProdutos();
