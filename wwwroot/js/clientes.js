Ui.montarLayout('clientes');

const elLista = document.getElementById('lista');
const fBusca = document.getElementById('f-busca');
let debounce;

async function carregar() {
  Ui.loading(elLista);
  const params = new URLSearchParams();
  if (fBusca.value.trim()) params.append('busca', fBusca.value.trim());
  try {
    const clientes = await Api.get('/api/clientes?' + params.toString());
    render(clientes);
  } catch (e) {
    Ui.erro(e.message);
    Ui.vazio(elLista, 'Não foi possível carregar os clientes.', '⚠️');
  }
}

function render(clientes) {
  if (!clientes || clientes.length === 0) { Ui.vazio(elLista, 'Nenhum cliente encontrado.', '👤'); return; }
  elLista.innerHTML = `
    <div class="tabela-wrap">
      <table class="tabela">
        <thead><tr><th>Nome</th><th>Telefone</th><th>E-mail</th><th class="acoes">Ações</th></tr></thead>
        <tbody>
          ${clientes.map(c => `
            <tr>
              <td>${Ui.escape(c.nome)}</td>
              <td>${Ui.escape(c.telefone || '—')}</td>
              <td>${Ui.escape(c.email || '—')}</td>
              <td class="acoes">
                <a class="btn-icon btn-icon--editar" title="Editar" href="/pages/cliente-form.html?id=${c.id}">${Ui.icones.editar}</a>
                <button class="btn-icon btn-icon--excluir" title="Excluir" data-excluir="${c.id}" data-nome="${Ui.escape(c.nome)}">${Ui.icones.excluir}</button>
              </td>
            </tr>`).join('')}
        </tbody>
      </table>
    </div>`;
  elLista.querySelectorAll('[data-excluir]').forEach(b =>
    b.addEventListener('click', () => excluir(b.dataset.excluir, b.dataset.nome)));
}

async function excluir(id, nome) {
  const ok = await Ui.confirmar(`Excluir o cliente "${nome}"?`, 'Excluir');
  if (!ok) return;
  try {
    await Api.del('/api/clientes/' + id);
    Ui.sucesso('Cliente excluído.');
    carregar();
  } catch (e) { Ui.erro(e.message); }
}

fBusca.addEventListener('input', () => { clearTimeout(debounce); debounce = setTimeout(carregar, 300); });
carregar();
