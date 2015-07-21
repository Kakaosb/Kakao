class UsersController < ApplicationController
before_action :signed_in_user, only: [:index, :edit, :update, :destroy]
before_action :correct_user,   only: [:edit, :update]
before_action :admin_user,     only: :destroy

def index
    @users = User.paginate(page: params[:page])
    #@users = User.all
    #@users = User.paginate(page: params[:page])
  end
	 def show
    @user = User.find(params[:id])
     @articles = @user.articles.paginate(page: params[:page])
  end

	def new
    @user = User.new
  end

def edit
  @user = User.find(params[:id])
  end

  def create
  @user = User.new(user_params)
 #Rails.logger.debug @user.inspect
  if @user.save
    sign_in @user
      flash[:success] = "Добро пожаловать!"
      redirect_to @user
    else
      render 'new'
    end
end
 
   def update
    @user = User.find(params[:id])
    if @user.update_attributes(user_params)
      flash[:success] = "Профиль изменен"
      redirect_to @user
    else
      render 'edit'
    end
  end

def destroy
  @user = User.find(params[:id]).destroy
    redirect_to users_path
end

private
  def user_params
    params.require(:user).permit(:nick, :login, :password, :email, :password_confirmation)
  end

   # Before filters

  

 def correct_user
      @user = User.find(params[:id])
      redirect_to(root_url) unless current_user?(@user)
    end

    def admin_user
      redirect_to(root_url) unless current_user.admin?
    end

end
