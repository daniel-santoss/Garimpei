/* ============================================================
   Api — wrapper de fetch com tratamento do envelope de erro.
   Todo erro da API vem como { sucesso:false, mensagem:"..." }.
   ============================================================ */

class ApiError extends Error {
  constructor(mensagem, status) {
    super(mensagem);
    this.name = 'ApiError';
    this.status = status;
  }
}

const Api = {
  async request(metodo, url, body) {
    const opcoes = { method: metodo, headers: {} };
    if (body !== undefined && body !== null) {
      opcoes.headers['Content-Type'] = 'application/json';
      opcoes.body = JSON.stringify(body);
    }

    let res;
    try {
      res = await fetch(url, opcoes);
    } catch (e) {
      throw new ApiError('Não foi possível conectar ao servidor.', 0);
    }

    const texto = await res.text();
    let dados = null;
    if (texto) {
      try { dados = JSON.parse(texto); } catch { dados = texto; }
    }

    if (!res.ok) {
      const msg = (dados && dados.mensagem) ? dados.mensagem : 'Ocorreu um erro inesperado.';
      throw new ApiError(msg, res.status);
    }
    return dados;
  },

  get(url) { return this.request('GET', url); },
  post(url, body) { return this.request('POST', url, body); },
  put(url, body) { return this.request('PUT', url, body); },
  patch(url, body) { return this.request('PATCH', url, body); },
  del(url) { return this.request('DELETE', url); },
};
