class WelcomeController < ApplicationController
  def index
  	
  	    @articles = Article.paginate(page: params[:page], :per_page => 15)
  #@articles = Article.all
   if signed_in?
      @article  = current_user.articles.build
     
   
  end
  end

  def contact
  end
end
