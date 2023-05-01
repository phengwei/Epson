<template>
    <!-- Mobile menu -->
    <transition
        enter-active-class="transform transition duration-500 ease-custom"
        enter-class="-translate-x-1/2 scale-x-0 opacity-0"
        enter-to-class="translate-x-0 scale-x-100 opacity-100"
        leave-active-class="transform transition duration-300 ease-custom"
        leave-class="translate-x-0 scale-x-100 opacity-100"
        leave-to-class="-translate-x-1/2 scale-x-0 opacity-0"
    >
        <div v-show="showPopup" @close-modal="closeModal" class="w-screen h-screen">
            <div class="modal-overlay flex fixed inset-0 justify-center items-center sm:items-start w-screen h-screen bg-[#19212b] z-40" >
                <div class="modal-overlay absolute w-full h-full bg-#19212b opacity-95"></div>

                <div class="modal flex flex-col relative text-center w-full h-full overflow-y-auto" @click.stop>
                
                    <div class="modal-header fixed inset-x-0 top-0 h-16 w-full bg-[#19212b] z-30 border-b-[1px] border-white">
                        <div class="modal-close absolute top-0 left-0 cursor-pointer flex flex-col items-center mt-4 ml-4 text-white text-sm z-50">
                        <div class="w-6 h-6">
                            <button @click="closeModal">
                                <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="currentColor" class="bi bi-x" viewBox="0 0 16 16"> <path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z"/> </svg>
                            </button>
                        </div>
                        </div>
                    </div>
                    <div class="modal-content container mx-auto h-auto mt-16 mb-16 text-center">
                        <!--Body-->
                        <ul class="text-left">
                            <li><nuxt-link to="/" class="block  px-2 py-8 text-white hover:text-white hover:bg-[#003399] font-semibold transition duration-300">{{ loggedInUser.userName }}</nuxt-link></li>
                            <hr />
                            <li><nuxt-link to="/dashboard" class="block text-white font-semibold px-2 py-8 hover:text-white hover:bg-[#003399] transition duration-300">Dashboard</nuxt-link></li>
                            <hr />
                            <li><a target="_blank" class="block px-2 text-white font-semibold py-8 hover:text-white hover:bg-[#003399] transition duration-300" @click="logout">Logout</a></li>
                            <hr />
                            <li>
                                <div class="flex flex-row justify-start my-4 ml-2">
                                    <a class="bg-blue-500 p-2 font-semibold text-white inline-flex items-center space-x-2 rounded mx-2" href="https://www.facebook.com/EpsonMalaysia/" target="_blank">
                                        <svg class="w-5 h-5 fill-current" role="img" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><path d="M24 12.073c0-6.627-5.373-12-12-12s-12 5.373-12 12c0 5.99 4.388 10.954 10.125 11.854v-8.385H7.078v-3.47h3.047V9.43c0-3.007 1.792-4.669 4.533-4.669 1.312 0 2.686.235 2.686.235v2.953H15.83c-1.491 0-1.956.925-1.956 1.874v2.25h3.328l-.532 3.47h-2.796v8.385C19.612 23.027 24 18.062 24 12.073z" /></svg>
                                    </a>
                                    <!-- <a class="footer__social-item" href="https://www.instagram.com/epsonmalaysia/?hl=en" target="_blank">
                                    <span class="footer__contact-item-icon text-2xl">
                                        <fa :icon="['fab','instagram']" />
                                    </span>
                                    </a> -->
                                    <a class="bg-blue-600 p-2 font-semibold text-white inline-flex items-center space-x-2 rounded mx-2" href="https://www.linkedin.com/company/epson-malaysia/" target="_blank">
                                        <svg class="w-5 h-5 fill-current" role="img" viewBox="0 0 256 256" xmlns="http://www.w3.org/2000/svg">
                                            <g><path d="M218.123122,218.127392 L180.191928,218.127392 L180.191928,158.724263 C180.191928,144.559023 179.939053,126.323993 160.463756,126.323993 C140.707926,126.323993 137.685284,141.757585 137.685284,157.692986 L137.685284,218.123441 L99.7540894,218.123441 L99.7540894,95.9665207 L136.168036,95.9665207 L136.168036,112.660562 L136.677736,112.660562 C144.102746,99.9650027 157.908637,92.3824528 172.605689,92.9280076 C211.050535,92.9280076 218.138927,118.216023 218.138927,151.114151 L218.123122,218.127392 Z M56.9550587,79.2685282 C44.7981969,79.2707099 34.9413443,69.4171797 34.9391618,57.260052 C34.93698,45.1029244 44.7902948,35.2458562 56.9471566,35.2436736 C69.1040185,35.2414916 78.9608713,45.0950217 78.963054,57.2521493 C78.9641017,63.090208 76.6459976,68.6895714 72.5186979,72.8184433 C68.3913982,76.9473153 62.7929898,79.26748 56.9550587,79.2685282 M75.9206558,218.127392 L37.94995,218.127392 L37.94995,95.9665207 L75.9206558,95.9665207 L75.9206558,218.127392 Z M237.033403,0.0182577091 L18.8895249,0.0182577091 C8.57959469,-0.0980923971 0.124827038,8.16056231 -0.001,18.4706066 L-0.001,237.524091 C0.120519052,247.839103 8.57460631,256.105934 18.8895249,255.9977 L237.033403,255.9977 C247.368728,256.125818 255.855922,247.859464 255.999,237.524091 L255.999,18.4548016 C255.851624,8.12438979 247.363742,-0.133792868 237.033403,0.000790807055"></path></g>
                                        </svg>
                                    </a>
                                    <a class="bg-red-600 p-2 font-semibold text-white inline-flex items-center space-x-2 rounded mx-2" href="https://www.youtube.com/c/EpsonSoutheastAsia" target="_blank">
                                        <svg class="w-5 h-5 fill-current" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16"><path d="M8.051 1.999h.089c.822.003 4.987.033 6.11.335a2.01 2.01 0 0 1 1.415 1.42c.101.38.172.883.22 1.402l.01.104.022.26.008.104c.065.914.073 1.77.074 1.957v.075c-.001.194-.01 1.108-.082 2.06l-.008.105-.009.104c-.05.572-.124 1.14-.235 1.558a2.007 2.007 0 0 1-1.415 1.42c-1.16.312-5.569.334-6.18.335h-.142c-.309 0-1.587-.006-2.927-.052l-.17-.006-.087-.004-.171-.007-.171-.007c-1.11-.049-2.167-.128-2.654-.26a2.007 2.007 0 0 1-1.415-1.419c-.111-.417-.185-.986-.235-1.558L.09 9.82l-.008-.104A31.4 31.4 0 0 1 0 7.68v-.123c.002-.215.01-.958.064-1.778l.007-.103.003-.052.008-.104.022-.26.01-.104c.048-.519.119-1.023.22-1.402a2.007 2.007 0 0 1 1.415-1.42c.487-.13 1.544-.21 2.654-.26l.17-.007.172-.006.086-.003.171-.007A99.788 99.788 0 0 1 7.858 2h.193zM6.4 5.209v4.818l4.157-2.408L6.4 5.209z"></path></svg>
                                    </a>
                                </div>
                            </li>
                            <hr />
                        </ul>
                    </div>
                </div>
            </div>
        </div>
    </transition>
    
</template>

<script>
import { mapGetters } from 'vuex';
import EventBus from '~/components/eventbus'

export default {
    name: 'MobileNav',
    data () {
      return {
        showPopup: false
      }
    },
    computed: {
        ...mapGetters(['isAuthenticated', 'loggedInUser'])
    },
    methods: {
        closeModal () {
            this.showPopup = false
            document.body.classList.remove('stop-scrolling')
            EventBus.$emit('CLOSE_MOBILE_HEADER', this.showPopup)
        },
        async logout(){
            await this.$auth.logout().then(response => {
                this.$router.push("/login");
            })
        }
    },
    mounted () {
        const $vm = this
        EventBus.$on('OPEN_MOBILE_HEADER', function (showPopup) {
            $vm.showPopup = showPopup
        })
        EventBus.$on('CLOSE_MOBILE_HEADER', function (showPopup) {
            $vm.showPopup = showPopup
        })
    }
}
</script>

<style>
.stop-scrolling {
  height: 100%;
  overflow: hidden
}
.fa-facebook-f{
    color: #2D609B;
}

.homePageIsActiveMobile {
    background-color: #003399;
    color:white
}
</style>