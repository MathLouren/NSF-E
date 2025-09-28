import { createRouter, createWebHistory } from "vue-router"
import Dashboard from "@/pages/Dashboard.vue"
import Nova from "@/pages/Nfse/Nova.vue"
import Lista from "@/pages/Nfse/Lista.vue"
import EmitirNFe from "@/pages/Nfe/Emitir.vue"
import ListaNFe from "@/pages/Nfe/Lista.vue"
import ConfiguracaoNFe from "@/pages/Nfe/Configuracao.vue"
import DanfeVersao from "@/pages/Nfe/DanfeVersao.vue"
import Detalhe from "@/pages/Nfse/Detalhe.vue"
import Configuracoes from "@/pages/Configuracoes.vue"
import Login from "@/pages/Login.vue"
import NewProduct from "@/pages/Products/NewProduct.vue"
import NewService from "@/pages/Services/NewService.vue"
import ListProducts from "@/pages/Products/ListProducts.vue"
import ListServices from "@/pages/Services/ListServices.vue"
import ProductDetail from "@/pages/Products/ProductDetail.vue"
import ServiceDetail from "@/pages/Services/ServiceDetail.vue"

const routes = [
  { path: "/", redirect: "/dashboard" },
  { path: "/login", component: Login },
  { path: "/dashboard", component: Dashboard },
  { path: "/nfse/nova", component: Nova },
  { path: "/nfse/lista", component: Lista },
  { path: "/nfe/emitir", component: EmitirNFe },
  { path: "/nfe/lista", component: ListaNFe },
  { path: "/nfe/configuracao", component: ConfiguracaoNFe },
  { path: "/nfe/danfe-versao", component: DanfeVersao },
  { path: "/nfse/:id", component: Detalhe },
  { path: "/configuracoes", component: Configuracoes },
  { path: "/products/new", component: NewProduct },
  { path: "/products/list", component: ListProducts },
  { path: "/products/:id", component: ProductDetail },
  { path: "/services/new", component: NewService },
  { path: "/services/list", component: ListServices },
  { path: "/services/:id", component: ServiceDetail }
]

export default createRouter({
  history: createWebHistory(),
  routes,
})